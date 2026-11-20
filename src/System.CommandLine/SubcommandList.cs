// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

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
                string? first = null;
                List<string>? multiple = null;

                for (int i = 0; i < aliases.Length; i++)
                {
                    string alias = aliases[i] ?? throw new ArgumentNullException(nameof(aliases));
                    Symbol.ThrowIfEmptyOrWithWhitespaces(alias, nameof(aliases));

                    if (string.Equals(alias, Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (first is null)
                    {
                        first = alias;
                    }
                    else if (multiple is null)
                    {
                        if (!string.Equals(first, alias, StringComparison.Ordinal))
                        {
                            multiple = new List<string>(aliases.Length) { first, alias };
                        }
                    }
                    else if (!multiple.Contains(alias))
                    {
                        multiple.Add(alias);
                    }
                }

                _aliases = multiple is not null ? multiple.ToArray() : first;
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
                catch
                {
                    _state = factory;
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
                    if (!Matches(existingAlias, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"The subcommand '{Name}' defines alias '{existingAlias}' which was not declared when registering the subcommand.");
                    }
                }
            }

            if (_aliases is string singleAlias)
            {
                command.Aliases.Add(singleAlias);
            }
            else if (_aliases is string[] multipleAliases)
            {
                for (int i = 0; i < multipleAliases.Length; i++)
                {
                    command.Aliases.Add(multipleAliases[i]);
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

        internal bool TryGetSubcommand(string nameOrAlias, StringComparison comparisonType, [NotNullWhen(true)] out Command? subcommand)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                object entry = _items[i];
                if (entry is Command eager)
                {
                    if (eager.EqualsNameOrAlias(nameOrAlias, comparisonType))
                    {
                        subcommand = eager;
                        return true;
                    }
                }
                else
                {
                    LazySubcommand lazy = (LazySubcommand)entry;
                    if (lazy.CreatedCommand is { } created)
                    {
                        if (created.EqualsNameOrAlias(nameOrAlias, comparisonType))
                        {
                            subcommand = created;
                            return true;
                        }
                    }
                    else if (lazy.Matches(nameOrAlias, comparisonType))
                    {
                        subcommand = lazy.GetOrCreate(_parent);
                        return true;
                    }
                }
            }

            subcommand = null;
            return false;
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

        public IEnumerator<Command> GetEnumerator()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                yield return this[i];
            }
        }

        public int IndexOf(Command item)
        {
            if (item is null)
            {
                return -1;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (ReferenceEquals(TryGetMaterialized(i), item))
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
    }
}
