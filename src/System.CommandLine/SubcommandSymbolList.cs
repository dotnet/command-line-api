// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.CommandLine
{
    internal sealed class DeferredSubcommand
    {
        private readonly Lazy<Command> _lazy;

        internal DeferredSubcommand(Command parent, string name, ReadOnlySpan<string> aliases, Func<Command> subcommandFactory)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = Symbol.ThrowIfEmptyOrWithWhitespaces(name, nameof(name));
            if (subcommandFactory is null)
            {
                throw new ArgumentNullException(nameof(subcommandFactory));
            }

            if (aliases.Length == 0)
            {
                Aliases = Array.Empty<string>();
            }
            else
            {
                string[] copy = new string[aliases.Length];
                int count = 0;

                for (int i = 0; i < aliases.Length; i++)
                {
                    string alias = Symbol.ThrowIfEmptyOrWithWhitespaces(aliases[i], nameof(aliases));
                    if (string.Equals(alias, Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool duplicate = false;
                    for (int j = 0; j < count; j++)
                    {
                        if (string.Equals(copy[j], alias, StringComparison.Ordinal))
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (!duplicate)
                    {
                        copy[count++] = alias;
                    }
                }

                if (count != copy.Length)
                {
                    Array.Resize(ref copy, count);
                }

                Aliases = copy;
            }

            _lazy = new Lazy<Command>(() => Materialize(parent, subcommandFactory), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal string Name { get; }

        internal string[] Aliases { get; }

        internal bool IsValueCreated => _lazy.IsValueCreated;

        internal Command GetOrCreate() => _lazy.Value;

        internal bool Matches(string nameOrAlias, StringComparison comparisonType)
        {
            if (string.Equals(Name, nameOrAlias, comparisonType))
            {
                return true;
            }

            for (int i = 0; i < Aliases.Length; i++)
            {
                if (string.Equals(Aliases[i], nameOrAlias, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        private Command Materialize(Command parent, Func<Command> subcommandFactory)
        {
            Command? command = subcommandFactory();
            if (command is null)
            {
                throw new InvalidOperationException($"The subcommand factory for '{Name}' returned null.");
            }

            if (!string.Equals(command.Name, Name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The subcommand factory registered for '{Name}' returned a command named '{command.Name}'.");
            }

            if (command._aliases is not null)
            {
                foreach (string existingAlias in command._aliases)
                {
                    if (Array.IndexOf(Aliases, existingAlias) < 0 &&
                        !string.Equals(existingAlias, Name, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"The subcommand '{Name}' defines alias '{existingAlias}' which was not declared when registering the subcommand.");
                    }
                }
            }

            for (int i = 0; i < Aliases.Length; i++)
            {
                command.Aliases.Add(Aliases[i]);
            }

            command.AddParent(parent);
            return command;
        }
    }

    internal sealed class SubcommandSymbolList : IList<Command>
    {
        private readonly struct Entry
        {
            internal Entry(Command eager)
            {
                Eager = eager;
                Deferred = null;
            }

            internal Entry(DeferredSubcommand deferred)
            {
                Eager = null;
                Deferred = deferred;
            }

            internal Command? Eager { get; }

            internal DeferredSubcommand? Deferred { get; }

            internal Command Resolve() => Eager ?? Deferred!.GetOrCreate();

            internal Command? TryGetMaterialized() => Eager ?? (Deferred!.IsValueCreated ? Deferred.GetOrCreate() : null);
        }

        private readonly List<Entry> _entries = new();
        private readonly Command _parent;

        internal SubcommandSymbolList(Command parent)
        {
            _parent = parent;
        }

        public Command this[int index]
        {
            get => _entries[index].Resolve();
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (_entries[index].TryGetMaterialized() is { } existing)
                {
                    existing.RemoveParent(_parent);
                }

                _entries[index] = new Entry(value);
                value.AddParent(_parent);
            }
        }

        public int Count => _entries.Count;

        public bool IsReadOnly => false;

        internal Command? TryGetMaterialized(int index) => _entries[index].TryGetMaterialized();

        internal string GetEntryName(int index)
        {
            Entry entry = _entries[index];
            return entry.Eager is { } eager ? eager.Name : entry.Deferred!.Name;
        }

        public void Add(Command item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.AddParent(_parent);
            _entries.Add(new Entry(item));
        }

        internal void AddDeferred(string name, ReadOnlySpan<string> aliases, Func<Command> subcommandFactory)
        {
            _entries.Add(new Entry(new DeferredSubcommand(_parent, name, aliases, subcommandFactory)));
        }

        internal bool ContainsSubcommand(string nameOrAlias, StringComparison comparisonType)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry.Eager is { } eager)
                {
                    if (eager.EqualsNameOrAlias(nameOrAlias, comparisonType))
                    {
                        return true;
                    }
                }
                else if (entry.Deferred!.IsValueCreated)
                {
                    if (entry.Deferred.GetOrCreate().EqualsNameOrAlias(nameOrAlias, comparisonType))
                    {
                        return true;
                    }
                }
                else if (entry.Deferred.Matches(nameOrAlias, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool TryGetSubcommand(string nameOrAlias, StringComparison comparisonType, [NotNullWhen(true)] out Command? subcommand)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry.Eager is { } eager)
                {
                    if (eager.EqualsNameOrAlias(nameOrAlias, comparisonType))
                    {
                        subcommand = eager;
                        return true;
                    }
                }
                else if (entry.Deferred!.IsValueCreated)
                {
                    Command materialized = entry.Deferred.GetOrCreate();
                    if (materialized.EqualsNameOrAlias(nameOrAlias, comparisonType))
                    {
                        subcommand = materialized;
                        return true;
                    }
                }
                else if (entry.Deferred.Matches(nameOrAlias, comparisonType))
                {
                    subcommand = entry.Deferred.GetOrCreate();
                    return true;
                }
            }

            subcommand = null;
            return false;
        }

        internal void AddEntryTokens(Dictionary<string, Token> tokens, int index)
        {
            Entry entry = _entries[index];
            if (entry.TryGetMaterialized() is { } cmd)
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
                DeferredSubcommand deferred = entry.Deferred!;
                tokens.Add(deferred.Name, new Token(deferred.Name, deferred));

                string[] aliases = deferred.Aliases;
                for (int i = 0; i < aliases.Length; i++)
                {
                    tokens.Add(aliases[i], new Token(aliases[i], deferred));
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TryGetMaterialized() is { } existing)
                {
                    existing.RemoveParent(_parent);
                }
            }

            _entries.Clear();
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

            if (array.Length - arrayIndex < _entries.Count)
            {
                throw new ArgumentException("Destination array was not long enough.");
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                array[arrayIndex + i] = _entries[i].Resolve();
            }
        }

        public IEnumerator<Command> GetEnumerator()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                yield return _entries[i].Resolve();
            }
        }

        public int IndexOf(Command item)
        {
            if (item is null)
            {
                return -1;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                if (Equals(_entries[i].TryGetMaterialized(), item))
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
            _entries.Insert(index, new Entry(item));
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
            if (_entries[index].TryGetMaterialized() is { } existing)
            {
                existing.RemoveParent(_parent);
            }

            _entries.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
