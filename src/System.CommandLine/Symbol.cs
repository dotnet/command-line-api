// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Diagnostics;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a named symbol that resides in a hierarchy with parent and child symbols.
    /// </summary>
    public abstract class Symbol
    {
        private protected Symbol(string name, bool allowWhitespace = false)
        {
            Name = ThrowIfEmptyOrWithWhitespaces(name, nameof(name), allowWhitespace);
        }

        /// <summary>
        /// Gets or sets the description of the symbol.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets the name of the symbol.
        /// </summary>
        public string Name { get; }

        private volatile SymbolNode? _firstParent;

        /// <summary>
        /// Represents the first parent node.
        /// </summary>
        internal SymbolNode? FirstParent => _firstParent;

        internal void AddParent(Symbol symbol)
        {
            SymbolNode newNode = new(symbol);
            if (Interlocked.CompareExchange(ref _firstParent, newNode, null) is null)
            {
                return;
            }

            lock (this)
            {
                if (_firstParent is null)
                {
                    _firstParent = newNode;
                }
                else
                {
                    SymbolNode current = _firstParent;
                    while (current.Next is not null)
                    {
                        current = current.Next;
                    }
                    current.Next = newNode;
                }
            }
        }

        internal void RemoveParent(Symbol symbol)
        {
            lock (this)
            {
                SymbolNode? previous = null;
                SymbolNode? current = _firstParent;
                while (current is not null)
                {
                    if (ReferenceEquals(current.Symbol, symbol))
                    {
                        if (previous is null)
                        {
                            _firstParent = current.Next;
                        }
                        else
                        {
                            previous.Next = current.Next;
                        }
                        return;
                    }

                    previous = current;
                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol is hidden.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets the parent symbols.
        /// </summary>
        public IEnumerable<Symbol> Parents
        {
            get
            {
                SymbolNode? parent = FirstParent;
                while (parent is not null)
                {
                    yield return parent.Symbol;
                    parent = parent.Next;
                }
            }
        }

        /// <summary>
        /// Gets completions for the symbol.
        /// </summary>
        public abstract IEnumerable<CompletionItem> GetCompletions(CompletionContext context);

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}: {Name}";

        [DebuggerStepThrough]
        internal static string ThrowIfEmptyOrWithWhitespaces(string value, string paramName, bool canContainWhitespaces = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Names and aliases cannot be null, empty, or consist entirely of whitespace.");
            }

            if (!canContainWhitespaces)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    if (char.IsWhiteSpace(value[i]))
                    {
                        throw new ArgumentException($"Names and aliases cannot contain whitespace: \"{value}\"", paramName);
                    }
                }
            }

            return value;
        }
    }
}