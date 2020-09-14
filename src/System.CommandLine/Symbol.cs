// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    public abstract class Symbol : 
        ISymbol
    {
        private protected readonly HashSet<string> _aliases = new HashSet<string>();
        private protected readonly HashSet<string> _rawAliases = new HashSet<string>();
        private protected string? _specifiedName;

        private readonly SymbolSet _parents = new SymbolSet();

        private protected Symbol()
        {
        }

        protected Symbol(string? description = null)
        {
            Description = description;
        }

        protected Symbol(string name, string? description = null)
        {
            _specifiedName = name;
            Description = description;
        }

        public IReadOnlyCollection<string> Aliases => _aliases;

        public IReadOnlyCollection<string> RawAliases => _rawAliases;

        public string? Description { get; set; }

        public virtual string Name
        {
            get => _specifiedName ?? DefaultName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
                }

                _specifiedName = value;

                OnNameOrAliasChanged?.Invoke(this);
            }
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

        public bool HasAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            return _aliases.Contains(alias.RemovePrefix());
        }
  
        public bool HasRawAlias(string alias) => _rawAliases.Contains(alias);

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
                   .SelectMany(s => s.RawAliases)
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
    }
}
