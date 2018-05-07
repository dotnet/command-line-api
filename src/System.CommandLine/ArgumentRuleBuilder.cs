// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ArgumentRuleBuilder
    {
        private ArgumentSuggestionSource suggestionSource;

        internal ArgumentArity ArgumentArity { get; set; }

        internal ConvertArgument ConvertArguments { get; set; }

        internal Func<string> DefaultValue { get; set; }

        internal ArgumentsRuleHelp Help { get; set; }

        internal ArgumentParser Parser { get; private set; }

        internal List<ValidateSymbol> SymbolValidators { get; set; } = new List<ValidateSymbol>();

        internal ArgumentSuggestionSource SuggestionSource =>
            suggestionSource ??
            (suggestionSource = new ArgumentSuggestionSource());

        internal HashSet<string> ValidTokens { get; } = new HashSet<string>();

        public void AddValidator(ValidateSymbol validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            SymbolValidators.Add(validator);
        }

        protected virtual ArgumentParser BuildArgumentParser()
        {
            var parser = new ArgumentParser(
                ArgumentArity,
                ConvertArguments);

            return parser;
        }

        public ArgumentsRule Build()
        {
            AddTokenValidator();

            return new ArgumentsRule(
                Parser ?? (Parser = BuildArgumentParser()),
                DefaultValue,
                Help,
                SymbolValidators,
                suggestionSource);
        }


        private void AddTokenValidator()
        {
            if (ValidTokens.Count == 0)
            {
                return;
            }

            AddValidator(parsedSymbol =>
            {
                if (parsedSymbol.Arguments.Count == 0)
                {
                    return null;
                }

                foreach (var arg in parsedSymbol.Arguments)
                {
                    if (!ValidTokens.Any(value => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ValidationMessages.UnrecognizedArgument(arg, ValidTokens);
                    }
                }

                return null;
            });
        }

        internal static ArgumentRuleBuilder From(ArgumentsRule arguments)
        {
            // TODO: (From) get rid of this method

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var suggestionSource = new ArgumentSuggestionSource();
            suggestionSource.AddSuggestionSource(arguments.SuggestionSource.Suggest);

            var builder = new ArgumentRuleBuilder
            {
                ConvertArguments = arguments.Parser.ConvertArguments,
                DefaultValue = arguments.GetDefaultValue,
                Help = new ArgumentsRuleHelp(
                    arguments.Help?.Name,
                    arguments.Help?.Description),
                Parser = arguments.Parser,
                suggestionSource = suggestionSource,
                SymbolValidators = new List<ValidateSymbol>(arguments.SymbolValidators)
            };

            return builder;
        }
    }
}
