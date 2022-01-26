// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a named symbol that resides in a hierarchy with parent and child symbols.
    /// </summary>
    public abstract class Symbol : ICompletionSource
    {
        private string? _name;
        private ParentNode? _firstParent;

        private protected Symbol()
        {
        }

        /// <summary>
        /// Gets or sets the description of the symbol.
        /// </summary>
        public virtual string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the symbol.
        /// </summary>
        public virtual string Name
        {
            get => _name ??= DefaultName;
            set => _name = value;
        }

        private protected abstract string DefaultName { get; }

        /// <summary>
        /// Represents the first parent node.
        /// </summary>
        internal ParentNode? FirstParent => _firstParent;
        
        internal void AddParent(Symbol symbol)
        {
            if (_firstParent == null)
            {
                _firstParent = new ParentNode(symbol);
            }
            else
            {
                ParentNode current = _firstParent;
                while (current.Next is not null)
                {
                    current = current.Next;
                }
                current.Next = new ParentNode(symbol);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets the parent symbols.
        /// </summary>
        public IEnumerable<Symbol> Parents
        {
            get
            {
                ParentNode? parent = _firstParent;
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
        public IEnumerable<CompletionItem> GetCompletions() => 
            GetCompletions(CompletionContext.Empty());

        /// <inheritdoc />
        public abstract IEnumerable<CompletionItem> GetCompletions(CompletionContext context);

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}: {Name}";
    }
}