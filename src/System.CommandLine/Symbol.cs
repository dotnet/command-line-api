// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class Symbol : ISymbol
    {
        private readonly HashSet<string> _aliases = new HashSet<string>();
        private readonly HashSet<string> _rawAliases = new HashSet<string>();
        private string _longestAlias = "";
        private string _specifiedName;
        private protected readonly ArgumentSet _arguments = new ArgumentSet();
        private readonly List<Symbol> _parents = new List<Symbol>();

        private protected Symbol()
        {
        }

        protected Symbol(
            IReadOnlyCollection<string> aliases,
            string description = null,
            Argument argument = null,
            bool isHidden = false)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (!aliases.Any())
            {
                throw new ArgumentException("An option must have at least one alias.");
            }

            foreach (var alias in aliases)
            {
                AddAlias(alias);
            }

            Description = description;

            IsHidden = isHidden;

            if (argument != null)
            {
                AddArgumentInner(argument);
            }
        }

        public IReadOnlyCollection<string> Aliases => _aliases;

        public IReadOnlyCollection<string> RawAliases => _rawAliases;

        public string Description { get; set; }

        public virtual string Name
        {
            get => _specifiedName ?? _longestAlias;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
                }

                if (value.Length != value.RemovePrefix().Length)
                {
                    throw new ArgumentException($"Property {GetType().Name}.{nameof(Name)} cannot have a prefix.");
                }

                _specifiedName = value;
            }
        }

        internal IReadOnlyList<Symbol> Parents => _parents; 

        private protected void AddParent(Symbol symbol)
        {
            _parents.Add(symbol);
        }

        private protected void AddSymbol(Symbol symbol)
        {
            if (this is Command command)
            {
                symbol.AddParent(command);
            }

            Children.Add(symbol);
        }

        private protected void AddArgumentInner(Argument argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            argument.AddParent(this);

            if (string.IsNullOrEmpty(argument.Name))
            {
                argument.Name = _aliases.First().ToLower();
            }

            _arguments.Add(argument);

            Children.Add(argument);
        }

        public SymbolSet Children { get; } = new SymbolSet();

        public void AddAlias(string alias)
        {
            var unprefixedAlias = alias?.RemovePrefix();

            if (string.IsNullOrWhiteSpace(unprefixedAlias))
            {
                throw new ArgumentException("An alias cannot be null, empty, or consist entirely of whitespace.");
            }

            for (var i = 0; i < alias.Length; i++)
            {
                if (char.IsWhiteSpace(alias[i]))
                {
                    throw new ArgumentException($"{GetType().Name} alias cannot contain whitespace: \"{alias}\"");
                }
            }

            _rawAliases.Add(alias);
            _aliases.Add(unprefixedAlias);

            if (unprefixedAlias.Length > Name?.Length)
            {
                _longestAlias = unprefixedAlias;
            }
        }

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

        public virtual IEnumerable<string> Suggest(string textToMatch = null)
        {
            var argumentSuggestions =
                _arguments
                    .SelectMany(a => a.Suggest(textToMatch))
                    .ToArray();

            return this.ChildSymbolAliases()
                       .Concat(argumentSuggestions)
                       .Distinct()
                       .OrderBy(symbol => symbol)
                       .Containing(textToMatch);
        }

        public override string ToString() => $"{GetType().Name}: {Name}";

        ISymbolSet ISymbol.Children => Children;
    }
}
