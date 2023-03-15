// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a named symbol that resides in a hierarchy with parent and child symbols.
    /// </summary>
    public abstract class Symbol
    {
        private ParentNode? _firstParent;

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
        /// Gets or sets the <see cref="CliAction"/> for the symbol. The handler represents the action
        /// that will be performed when the symbol is invoked.
        /// </summary>
        /// <remarks>
        /// <para>Use one of the <see cref="SetHandler(Func{InvocationContext, Int32})" /> overloads to construct a handler.</para>
        /// <para>If the handler is not specified, parser errors will be generated for command line input that
        /// invokes this symbol.</para></remarks>
        public CliAction? Handler { get; set; }

        /// <summary>
        /// Sets a synchronous symbol handler. The handler should return an exit code.
        /// </summary>
        public void SetHandler(Func<InvocationContext, int> handler)
            => Handler = new AnonymousCliAction(handler);

        /// <summary>
        /// Sets an asynchronous symbol handler. The handler should return an exit code.
        /// </summary>
        public void SetHandler(Func<InvocationContext, CancellationToken, Task<int>> handler)
            => Handler = new AnonymousCliAction(handler);

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