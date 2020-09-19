// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine
{
    public abstract class Symbol : ISymbol
    {
        private string _name;
        private readonly SymbolSet _parents = new SymbolSet();

        private protected Symbol()
        {
        }

        public string? Description { get; set; }

        public virtual string Name
        {
            get => _name ??= DefaultName;
            set => _name = value;
        }

        private protected abstract string DefaultName { get; }

        public ISymbolSet Parents => _parents;

        internal void AddParent(Symbol symbol)
        {
            _parents.AddWithoutAliasCollisionCheck(symbol);
        }

        private protected virtual void AddSymbol(Symbol symbol)
        {
            Children.Add(symbol);
        }

        private protected void AddArgumentInner(Argument argument)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            argument.AddParent(this);

            Children.Add(argument);
        }

        public SymbolSet Children { get; } = new SymbolSet();

        public bool IsHidden { get; set; }

        public virtual IEnumerable<string?> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            var argumentSuggestions =
                Children
                    .OfType<IArgument>()
                    .SelectMany(a => a.GetSuggestions(parseResult, textToMatch))
                    .ToArray();

            return Children
                   .Where(s => !s.IsHidden)
                   .OfType<INamedSymbol>()
                   .SelectMany(s => s.Aliases)
                   .Concat(argumentSuggestions)
                   .Distinct()
                   .Containing(textToMatch)
                   .Where(symbol => symbol != null)
                   .OrderBy(symbol => symbol!.IndexOfCaseInsensitive(textToMatch))
                   .ThenBy(symbol => symbol, StringComparer.OrdinalIgnoreCase);
        }

        public override string ToString() => $"{GetType().Name}: {Name}";

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