// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class Symbol : ISymbol
    {
        private readonly HashSet<string> _aliases = new HashSet<string>();

        private readonly HashSet<string> _rawAliases;

        protected internal Symbol(
            IReadOnlyCollection<string> aliases,
            string description,
            Argument argument = null,
            HelpDetail help = null)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (!aliases.Any())
            {
                throw new ArgumentException("An option must have at least one alias.");
            }

            _rawAliases = new HashSet<string>(aliases);

            foreach (var alias in aliases)
            {
                var cleanedAlias = alias?.RemovePrefix();
                if (string.IsNullOrWhiteSpace(cleanedAlias))
                {
                    throw new ArgumentException("An option alias cannot be null, empty, or consist entirely of whitespace.");
                }

                _aliases.Add(cleanedAlias);
            }

            Description = description;

            Name = aliases
                   .Select(a => a.RemovePrefix())
                   .OrderBy(a => a.Length)
                   .Last();

            Argument = argument ?? Argument.None;

            Help = help ?? new HelpDetail(Name, Description, false);
        }

        public IReadOnlyCollection<string> Aliases => _aliases;

        public IReadOnlyCollection<string> RawAliases => _rawAliases;

        public Argument Argument { get; protected set; }

        public string Description { get; }

        public HelpDetail Help { get; }

        public string Name { get; }

        public Command Parent { get; internal set; }

        public SymbolSet Children { get; } = new SymbolSet();

        internal void AddAlias(string alias) => _rawAliases.Add(alias);

        public bool HasAlias(string alias) => _aliases.Contains(alias.RemovePrefix());

        public bool HasRawAlias(string alias) => _rawAliases.Contains(alias);

        public virtual IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            var symbolAliases = Children.Where<ISymbol>(symbol => !symbol.IsHidden())
                                       .SelectMany(symbol => symbol.RawAliases);

            var argumentSuggestions = Argument
                                      .SuggestionSource
                                      .Suggest(parseResult, position);

            return symbolAliases.Concat(argumentSuggestions)
                                .Distinct()
                                .OrderBy(symbol => symbol)
                                .Containing(parseResult.TextToMatch());
        }

        public override string ToString() => $"{GetType().Name}: {Name}";

        ICommand ISymbol.Parent => Parent;

        ISymbolSet ISymbol.Children => Children;
    }
}
