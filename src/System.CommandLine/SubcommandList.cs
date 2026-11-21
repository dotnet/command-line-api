// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Runtime.ExceptionServices;

namespace System.CommandLine
{
    internal sealed class LazySubcommand
    {
        private static readonly object s_creating = new();

        internal readonly string Name;
        private readonly object? _aliases; // null (0), string (1), or string[] (2+)
        private volatile object _state;    // Func<Command> -> s_creating -> Command

        internal LazySubcommand(string name, ReadOnlySpan<string> aliases, Func<Command> commandFactory)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = Symbol.ThrowIfEmptyOrWithWhitespaces(name, nameof(name));

            if (commandFactory is null)
            {
                throw new ArgumentNullException(nameof(commandFactory));
            }

            if (aliases.Length == 0)
            {
                _aliases = null;
            }
            else if (aliases.Length == 1)
            {
                string single = aliases[0] ?? throw new ArgumentNullException(nameof(aliases));
                Symbol.ThrowIfEmptyOrWithWhitespaces(single, nameof(aliases));
                _aliases = string.Equals(single, Name, StringComparison.Ordinal) ? null : single;
            }
            else
            {
                int uniqueCount = 0;

                for (int i = 0; i < aliases.Length; i++)
                {
                    string alias = aliases[i] ?? throw new ArgumentNullException(nameof(aliases));
                    Symbol.ThrowIfEmptyOrWithWhitespaces(alias, nameof(aliases));

                    if (string.Equals(alias, Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool isDuplicate = false;
                    for (int j = 0; j < i; j++)
                    {
                        if (string.Equals(aliases[j], alias, StringComparison.Ordinal))
                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (!isDuplicate)
                    {
                        uniqueCount++;
                    }
                }

                if (uniqueCount == 0)
                {
                    _aliases = null;
                }
                else if (uniqueCount == 1)
                {
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        if (!string.Equals(aliases[i], Name, StringComparison.Ordinal))
                        {
                            _aliases = aliases[i];
                            break;
                        }
                    }
                }
                else
                {
                    string[] uniqueAliases = new string[uniqueCount];
                    int count = 0;
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string alias = aliases[i];
                        if (string.Equals(alias, Name, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        bool isDuplicate = false;
                        for (int j = 0; j < count; j++)
                        {
                            if (string.Equals(uniqueAliases[j], alias, StringComparison.Ordinal))
                            {
                                isDuplicate = true;
                                break;
                            }
                        }

                        if (!isDuplicate)
                        {
                            uniqueAliases[count++] = alias;
                        }
                    }

                    _aliases = uniqueAliases;
                }
            }

            _state = commandFactory;
        }

        internal Command? CreatedCommand => _state as Command;

        internal Command GetOrCreate(Command parent)
        {
            if (_state is Command existing)
            {
                return existing;
            }

            return MaterializeSlow(parent);
        }

        internal bool Matches(string nameOrAlias, StringComparison comparisonType)
        {
            if (string.Equals(Name, nameOrAlias, comparisonType))
            {
                return true;
            }

            if (_aliases is string singleAlias)
            {
                return string.Equals(singleAlias, nameOrAlias, comparisonType);
            }

            if (_aliases is string[] multipleAliases)
            {
                for (int i = 0; i < multipleAliases.Length; i++)
                {
                    if (string.Equals(multipleAliases[i], nameOrAlias, comparisonType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void AddTokens(Dictionary<string, Token> tokens)
        {
            tokens.Add(Name, new Token(Name, TokenType.Command, null, Token.ImplicitPosition));

            if (_aliases is string singleAlias)
            {
                tokens.Add(singleAlias, new Token(singleAlias, TokenType.Command, null, Token.ImplicitPosition));
            }
            else if (_aliases is string[] multipleAliases)
            {
                for (int i = 0; i < multipleAliases.Length; i++)
                {
                    tokens.Add(multipleAliases[i], new Token(multipleAliases[i], TokenType.Command, null, Token.ImplicitPosition));
                }
            }
        }

        private Command MaterializeSlow(Command parent)
        {
            lock (this)
            {
                if (_state is Command existing)
                {
                    return existing;
                }

                if (_state is ExceptionDispatchInfo failure)
                {
                    failure.Throw();
                    throw new InvalidOperationException("Unreachable.");
                }

                if (ReferenceEquals(_state, s_creating))
                {
                    throw new InvalidOperationException(
                        $"The subcommand factory for '{Name}' recursively accessed itself before initialization completed.");
                }

                Func<Command> factory = (Func<Command>)_state;
                _state = s_creating;

                Command? command;
                try
                {
                    command = factory();
                    if (command is null)
                    {
                        throw new InvalidOperationException($"The subcommand factory for '{Name}' returned null.");
                    }

                    if (!string.Equals(command.Name, Name, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"The subcommand factory registered for '{Name}' returned a command named '{command.Name}'.");
                    }

                    lock (command)
                    {
                        ValidateAndApplyAliases(command);
                        command.AddParent(parent);
                    }
                }
                catch (Exception exception)
                {
                    _state = ExceptionDispatchInfo.Capture(exception);
                    throw;
                }

                _state = command;
                return command;
            }
        }

        private void ValidateAndApplyAliases(Command command)
        {
            if (command._aliases is not null)
            {
                foreach (string existingAlias in command._aliases)
                {
                    if (string.Equals(existingAlias, Name, StringComparison.Ordinal) ||
                        !Matches(existingAlias, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"The subcommand '{Name}' defines alias '{existingAlias}' which was not declared when registering the subcommand.");
                    }
                }
            }

            bool alreadyAttached = command.FirstParent is not null;
            if (_aliases is string singleAlias)
            {
                if (alreadyAttached)
                {
                    if (command._aliases?.Contains(singleAlias) != true)
                    {
                        throw new InvalidOperationException(
                            $"The subcommand '{Name}' is already attached and does not have the declared alias '{singleAlias}'.");
                    }
                }
                else
                {
                    command.Aliases.Add(singleAlias);
                }
            }
            else if (_aliases is string[] multipleAliases)
            {
                for (int i = 0; i < multipleAliases.Length; i++)
                {
                    string alias = multipleAliases[i];
                    if (alreadyAttached)
                    {
                        if (command._aliases?.Contains(alias) != true)
                        {
                            throw new InvalidOperationException(
                                $"The subcommand '{Name}' is already attached and does not have the declared alias '{alias}'.");
                        }
                    }
                    else
                    {
                        command.Aliases.Add(alias);
                    }
                }
            }
        }
    }

    internal sealed class SubcommandList : IList<Command>
    {
        // Each item is either an eager/replaced Command or a LazySubcommand (40B on 64-bit).
        private readonly List<object> _items = new();
        private readonly Command _parent;

        internal SubcommandList(Command parent) => _parent = parent;

        public Command this[int index]
        {
            get => _items[index] is Command command
                ? command
                : ((LazySubcommand)_items[index]).GetOrCreate(_parent);
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (TryGetMaterialized(index) is { } existing)
                {
                    existing.RemoveParent(_parent);
                }

                _items[index] = value;
                value.AddParent(_parent);
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public void Add(Command item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.AddParent(_parent);
            _items.Add(item);
        }

        internal void AddLazy(string name, ReadOnlySpan<string> aliases, Func<Command> commandFactory)
            => _items.Add(new LazySubcommand(name, aliases, commandFactory));

        internal Command? TryGetMaterialized(int index)
            => _items[index] is Command command ? command : ((LazySubcommand)_items[index]).CreatedCommand;

        internal int CountUnmaterializedByName(string name)
        {
            int count = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is LazySubcommand { CreatedCommand: null } lazy &&
                    string.Equals(lazy.Name, name, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        internal int CountEntriesByName(string name)
        {
            int count = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                string entryName = _items[i] is Command command ? command.Name : ((LazySubcommand)_items[i]).Name;
                if (string.Equals(entryName, name, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        internal Command GetLazy(string nameOrAlias)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is LazySubcommand lazy && lazy.Matches(nameOrAlias, StringComparison.Ordinal))
                {
                    return lazy.GetOrCreate(_parent);
                }
            }

            throw new InvalidOperationException($"Command '{_parent.Name}' has no lazy subcommand matching '{nameOrAlias}'.");
        }

        internal void AddEntryTokens(Dictionary<string, Token> tokens, int index)
        {
            if (TryGetMaterialized(index) is { } cmd)
            {
                tokens.Add(cmd.Name, new Token(cmd.Name, TokenType.Command, cmd, Token.ImplicitPosition));

                if (cmd._aliases is not null)
                {
                    foreach (string childAlias in cmd._aliases)
                    {
                        tokens.Add(childAlias, new Token(childAlias, TokenType.Command, cmd, Token.ImplicitPosition));
                    }
                }
            }
            else
            {
                ((LazySubcommand)_items[index]).AddTokens(tokens);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (TryGetMaterialized(i) is { } existing)
                {
                    existing.RemoveParent(_parent);
                }
            }

            _items.Clear();
        }

        public bool Contains(Command item) => IndexOf(item) >= 0;

        public void CopyTo(Command[] array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < _items.Count)
            {
                throw new ArgumentException("Destination array was not long enough.");
            }

            for (int i = 0; i < _items.Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public IEnumerator<Command> GetEnumerator() => new Enumerator(this);

        public int IndexOf(Command item)
        {
            if (item is null)
            {
                return -1;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (TryGetMaterialized(i) is { } command &&
                    EqualityComparer<Command>.Default.Equals(command, item))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, Command item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.AddParent(_parent);
            _items.Insert(index, item);
        }

        public bool Remove(Command item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (TryGetMaterialized(index) is { } existing)
            {
                existing.RemoveParent(_parent);
            }

            _items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Enumerator : IEnumerator<Command>
        {
            private readonly IEnumerator<object> _entries;
            private readonly Command _parent;
            private Command? _current;

            internal Enumerator(SubcommandList list)
            {
                _entries = list._items.GetEnumerator();
                _parent = list._parent;
            }

            public Command Current => _current!;

            object IEnumerator.Current
            {
                get
                {
                    _ = ((IEnumerator)_entries).Current;
                    return Current;
                }
            }

            public bool MoveNext()
            {
                if (!_entries.MoveNext())
                {
                    _current = null;
                    return false;
                }

                object entry = _entries.Current;
                _current = entry is Command command ? command : ((LazySubcommand)entry).GetOrCreate(_parent);
                return true;
            }

            public void Reset()
            {
                _entries.Reset();
                _current = null;
            }

            public void Dispose() => _entries.Dispose();
        }
    }
}
