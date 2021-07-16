// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine
{
    /// <inheritdoc />
    public abstract class Symbol : ISymbol
    {
        private string? _name;
        private readonly SymbolSet _parents = new();

        private protected Symbol()
        {
        }

        /// <summary>
        /// Gets or sets the description of the symbol.
        /// </summary>
        public string? Description { get; set; }

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
        /// Represents the parent symbols.
        /// </summary>
        public ISymbolSet Parents => _parents;

        internal void AddParent(Symbol symbol)
        {
            _parents.AddWithoutAliasCollisionCheck(symbol);
        }

        private protected virtual void AddSymbol(Symbol symbol)
        {
            Children.Add(symbol);
            symbol.AddParent(this);
        }

        private protected void AddArgumentInner(Argument argument)
        {
            argument.AddParent(this);
            Children.Add(argument);
        }

        /// <summary>
        /// Gets the child symbols.
        /// </summary>
        public SymbolSet Children { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the symbol is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            var suggestions = new HashSet<string>();

            textToMatch ??= "";

            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                switch (child)
                {
                    case IIdentifierSymbol identifier when !child.IsHidden:
                        foreach (var alias in identifier.Aliases)
                        {
                            if (alias is { } suggestion && 
                                suggestion.ContainsCaseInsensitive(textToMatch))
                            {
                                suggestions.Add(suggestion);
                            }
                        }

                        break;
                    case IArgument argument:
                        foreach (var suggestion in argument.GetSuggestions(parseResult, textToMatch))
                        {
                            if (suggestion is { } && 
                                suggestion.ContainsCaseInsensitive(textToMatch))
                            {
                                suggestions.Add(suggestion);
                            }
                        }

                        break;
                }
            }

            return suggestions
                   .OrderBy(symbol => symbol!.IndexOfCaseInsensitive(textToMatch))
                   .ThenBy(symbol => symbol, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}: {Name}";

        /// <inheritdoc />
        ISymbolSet ISymbol.Children => Children;

        internal Action<ISymbol>? OnNameOrAliasChanged;

        [DebuggerStepThrough]
        private protected void ThrowIfAliasIsInvalid(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("An alias cannot be null, empty, or consist entirely of whitespace.");
            }

            for (var i = 0; i < alias.Length; i++)
            {
                if (char.IsWhiteSpace(alias[i]))
                {
                    throw new ArgumentException($"{GetType().Name} alias cannot contain whitespace: \"{alias}\"", nameof(alias));
                }
            }
        }
    }
}