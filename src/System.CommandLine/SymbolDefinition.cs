// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolDefinition : ISuggestionSource
    {
        private readonly HashSet<string> _aliases = new HashSet<string>();

        private readonly HashSet<string> _rawAliases;

        protected internal SymbolDefinition(
            IReadOnlyCollection<string> aliases,
            string description,
            ArgumentDefinition argumentDefinition = null,
            HelpDefinition help = null)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (!aliases.Any())
            {
                throw new ArgumentException("An option must have at least one alias.");
            }

            if (aliases.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("An option alias cannot be null, empty, or consist entirely of whitespace.");
            }

            _rawAliases = new HashSet<string>(aliases);

            foreach (var alias in aliases)
            {
                _aliases.Add(alias.RemovePrefix());
            }

            ArgumentDefinition = argumentDefinition ?? ArgumentDefinition.None;
            Description = description;
            Help = help;
            Name = aliases
                   .Select(a => a.RemovePrefix())
                   .OrderBy(a => a.Length)
                   .Last();
        }

        public IReadOnlyCollection<string> Aliases => _aliases;

        public IReadOnlyCollection<string> RawAliases => _rawAliases;

        protected internal ArgumentDefinition ArgumentDefinition { get; protected set; }

        public string Description { get; }

        public HelpDefinition Help { get; }

        protected internal bool HasArguments => ArgumentDefinition != null && ArgumentDefinition != ArgumentDefinition.None;

        protected internal bool HasHelp => ArgumentDefinition != null && ArgumentDefinition.HasHelp;

        public string Name { get; }

        // FIX: (Parent) make this immutable
        public CommandDefinition Parent { get; protected internal set; }

        public SymbolDefinitionSet SymbolDefinitions { get; } = new SymbolDefinitionSet();

        internal void AddAlias(string alias) => _rawAliases.Add(alias);

        public bool HasAlias(string alias) => _aliases.Contains(alias.RemovePrefix());

        public bool HasRawAlias(string alias) => _rawAliases.Contains(alias);

        internal string Token() => _rawAliases.First(alias => alias.RemovePrefix() == Name);

        public virtual IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            var symbolAliases = SymbolDefinitions
                                .Where(s => !s.IsHidden())
                                .SelectMany(s => s.RawAliases);

            var argumentSuggestions = ArgumentDefinition.SuggestionSource
                                                   .Suggest(parseResult, position);

            return symbolAliases.Concat(argumentSuggestions)
                                .Distinct()
                                .OrderBy(s => s)
                                .Containing(parseResult.TextToMatch());
        }
    }
}
