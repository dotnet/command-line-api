// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Command : Symbol
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        public Command(
            IReadOnlyCollection<Symbol> symbols) :
            this(executableName.Value, "", symbols)
        {
        }

        public Command(
            string name,
            string description,
            ArgumentsRule arguments,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, arguments)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Symbol> symbols,
            ArgumentsRule arguments = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            var commandNames = symbols.SelectMany(o => o.Aliases).ToArray();

            var builder = new ArgumentRuleBuilder();
            //TODO: This need refinement to handle cases of options and sub commands
            ArgumentsRule = builder
                .WithSuggestions(commandNames)
                .ZeroOrMore();


            foreach (var option in symbols)
            {
                option.Parent = this;
                DefinedSymbols.Add(option);
            }

            ArgumentsRule.Parser.AddSuggetions(GetSuggestionsFromDefinedSymbols);


            //ArgumentsRule = ArgumentsRule.And(ZeroOrMoreOf(symbols.ToArray()));
        }

        private IEnumerable<string> GetSuggestionsFromDefinedSymbols(ParseResult parseresult, int? position)
        {
            return DefinedSymbols.Select(x => x.Name);
        }

        public bool TreatUnmatchedTokensAsErrors { get; } = true;

        public override string ToString() => Name;


    }
}
