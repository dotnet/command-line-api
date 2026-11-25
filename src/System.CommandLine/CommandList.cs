// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the subcommands of a <see cref="Command"/>, including subcommands that are registered lazily
    /// and created only when they are needed.
    /// </summary>
    /// <remarks>
    /// Members that return <see cref="Command"/> instances (the indexer, enumeration, <see cref="CopyTo"/>)
    /// create lazily registered subcommands on demand. The parser creates only the subcommands that were matched.
    /// </remarks>
    internal sealed class CommandList : IList<Command>
    {
        // Each item is either a Command or a LazyCommand.
        private readonly List<object> _items = new();
        private readonly Command _parent;

        internal CommandList(Command parent) => _parent = parent;

        /// <inheritdoc />
        public Command this[int index]
        {
            get => _items[index] switch
            {
                Command command => command,
                LazyCommand lazy => lazy.GetCommand(_parent),
                _ => throw new InvalidOperationException()
            };
            set
            {
                _items[index] = value;
                value.AddParent(_parent);
            }
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(Command item)
        {
            item.AddParent(_parent);
            _items.Add(item);
        }

        internal void AddLazy(string name, Func<Command> commandFactory) => AddLazy(name, ReadOnlySpan<string>.Empty, commandFactory);

        internal void AddLazy(string name, ReadOnlySpan<string> aliases, Func<Command> commandFactory)
        {
            Symbol.ThrowIfEmptyOrWithWhitespaces(name, nameof(name));

            for (int i = 0; i < aliases.Length; i++)
            {
                Symbol.ThrowIfEmptyOrWithWhitespaces(aliases[i], nameof(aliases));
            }

            if (commandFactory is null)
            {
                throw new ArgumentNullException(nameof(commandFactory));
            }

            _items.Add(new LazyCommand(name, aliases.ToArray(), commandFactory));
        }

        /// <inheritdoc />
        public void Clear() => _items.Clear();

        /// <inheritdoc />
        public bool Contains(Command item) => IndexOf(item) >= 0;

        /// <inheritdoc />
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
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection.", nameof(array));
            }

            for (int i = 0; i < _items.Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        /// <inheritdoc />
        public IEnumerator<Command> GetEnumerator()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                yield return this[i];
            }
        }

        /// <inheritdoc />
        public int IndexOf(Command item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                object current = _items[i];

                // A lazy command can be equal to item only when the names match, so other ones don't need to be created.
                if (current is LazyCommand lazy)
                {
                    if (item is null || !string.Equals(lazy.Name, item.Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    current = lazy.GetCommand(_parent);
                }

                if (Equals(current, item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, Command item)
        {
            item.AddParent(_parent);
            _items.Insert(index, item);
        }

        /// <inheritdoc />
        public bool Remove(Command item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            _items.RemoveAt(index);
            return true;
        }

        /// <inheritdoc />
        public void RemoveAt(int index) => _items.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns the subcommand at given index if it was already created, otherwise returns the name and aliases of the lazy subcommand.
        /// </summary>
        internal Command? GetIfCreated(int index, out string name, out string[] aliases)
        {
            if (_items[index] is LazyCommand lazy && lazy.CreatedCommand is null)
            {
                name = lazy.Name;
                aliases = lazy.Aliases;
                return null;
            }

            Command command = this[index];
            name = command.Name;
            aliases = Array.Empty<string>();
            return command;
        }

        /// <summary>
        /// Creates (if needed) and returns the lazy subcommand with given name or alias.
        /// </summary>
        internal Command GetLazy(string nameOrAlias)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is LazyCommand lazy && lazy.EqualsNameOrAlias(nameOrAlias))
                {
                    return lazy.GetCommand(_parent);
                }
            }

            throw new InvalidOperationException($"Command {_parent.Name} has no lazy subcommand named \"{nameOrAlias}\".");
        }

        private sealed class LazyCommand
        {
            private Func<Command>? _factory;
            private volatile Command? _command;

            internal LazyCommand(string name, string[] aliases, Func<Command> factory)
            {
                Name = name;
                Aliases = aliases;
                _factory = factory;
            }

            internal string Name { get; }

            internal string[] Aliases { get; }

            internal Command? CreatedCommand => _command;

            internal bool EqualsNameOrAlias(string nameOrAlias)
                => string.Equals(Name, nameOrAlias, StringComparison.Ordinal) || Array.IndexOf(Aliases, nameOrAlias) >= 0;

            internal Command GetCommand(Command parent)
            {
                if (_command is { } command)
                {
                    return command;
                }

                // The same root command can be used to parse input on multiple threads at the same time.
                lock (this)
                {
                    if (_command is null)
                    {
                        command = _factory!();

                        if (command is null)
                        {
                            throw new InvalidOperationException($"The factory for command \"{Name}\" returned null.");
                        }

                        if (!string.Equals(command.Name, Name, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException($"The factory for command \"{Name}\" returned a command named \"{command.Name}\".");
                        }

                        for (int i = 0; i < Aliases.Length; i++)
                        {
                            if (!command.Aliases.Contains(Aliases[i]))
                            {
                                command.Aliases.Add(Aliases[i]);
                            }
                        }

                        command.AddParent(parent);
                        _factory = null;
                        _command = command;
                    }

                    return _command;
                }
            }
        }
    }
}
