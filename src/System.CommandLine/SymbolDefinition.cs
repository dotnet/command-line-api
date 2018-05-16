// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolDefinition : ISuggestionSource
    {
        private readonly HashSet<string> aliases = new HashSet<string>();

        private readonly HashSet<string> rawAliases;

        protected internal SymbolDefinition(
            IReadOnlyCollection<string> aliases,
            string description,
            ArgumentDefinition argumentDefinition = null)
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

            rawAliases = new HashSet<string>(aliases);

            foreach (var alias in aliases)
            {
                this.aliases.Add(alias.RemovePrefix());
            }

            Description = description;

            Name = aliases
                   .Select(a => a.RemovePrefix())
                   .OrderBy(a => a.Length)
                   .Last();

            ArgumentDefinition = argumentDefinition ?? ArgumentDefinition.None;
        }

        public IReadOnlyCollection<string> Aliases => aliases;

        public IReadOnlyCollection<string> RawAliases => rawAliases;

        public SymbolDefinitionSet SymbolDefinitions { get; } = new SymbolDefinitionSet();

        public string Description { get; }

        protected internal ArgumentDefinition ArgumentDefinition { get; protected set; }

        public string Name { get; }

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

        // FIX: (Parent) make this immutable
        public CommandDefinition Parent { get; protected internal set; }

        public bool HasAlias(string alias) => aliases.Contains(alias.RemovePrefix());

        public bool HasRawAlias(string alias) => rawAliases.Contains(alias);

        public override string ToString() => RawAliases.First(alias => alias.RemovePrefix() == Name);

        internal void AddAlias(string alias) => rawAliases.Add(alias);
    }
}
