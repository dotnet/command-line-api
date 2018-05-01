// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public abstract class Symbol
    {
        private readonly HashSet<string> aliases = new HashSet<string>();

        private readonly HashSet<string> rawAliases;

        protected internal Symbol(
            IReadOnlyCollection<string> aliases,
            string description,
            ArgumentsRule arguments = null)
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

            ArgumentsRule = arguments ?? ArgumentsRule.None;

            //if (symbols != null)
            //{
            //    ArgumentsRule = ArgumentsRule.And(Accept.ZeroOrMoreOf(symbols.ToArray()));
            //}
        }

        public IReadOnlyCollection<string> Aliases => aliases;

        public IReadOnlyCollection<string> RawAliases => rawAliases;
        
        public SymbolSet DefinedSymbols { get; } = new SymbolSet();

        public string Description { get; }

        protected internal ArgumentsRule ArgumentsRule { get; protected set; }

        public string Name { get; }

        public IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null) => ArgumentsRule.Parser.Suggest(parseResult, position);

        // FIX: (Parent) make this immutable
        public Command Parent { get; protected internal set; }

        public bool HasAlias(string alias) => aliases.Contains(alias.RemovePrefix());

        public bool HasRawAlias(string alias) => rawAliases.Contains(alias);

        public Symbol this[string alias] => DefinedSymbols[alias];

        public override string ToString() => RawAliases.Single(a => a.RemovePrefix() == Name);
    }
}