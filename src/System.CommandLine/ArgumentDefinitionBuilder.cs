// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ArgumentDefinitionBuilder
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

        public ArgumentDefinition Build()
        {
            AddTokenValidator();

            return new ArgumentDefinition(
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

            AddValidator(symbol =>
            {
                if (symbol.Arguments.Count == 0)
                {
                    return null;
                }

                foreach (var arg in symbol.Arguments)
                {
                    if (!ValidTokens.Any(value => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ValidationMessages.UnrecognizedArgument(arg, ValidTokens);
                    }
                }

                return null;
            });
        }

        internal static ArgumentDefinitionBuilder From(ArgumentDefinition argumentDefinition)
        {
            // TODO: (From) get rid of this method

            if (argumentDefinition == null)
            {
                throw new ArgumentNullException(nameof(argumentDefinition));
            }

            var suggestionSource = new ArgumentSuggestionSource();
            suggestionSource.AddSuggestionSource(argumentDefinition.SuggestionSource.Suggest);

            var builder = new ArgumentDefinitionBuilder
            {
                ConvertArguments = argumentDefinition.Parser.ConvertArguments,
                DefaultValue = argumentDefinition.GetDefaultValue,
                Help = new ArgumentsRuleHelp(
                    argumentDefinition.Help?.Name,
                    argumentDefinition.Help?.Description),
                Parser = argumentDefinition.Parser,
                suggestionSource = suggestionSource,
                SymbolValidators = new List<ValidateSymbol>(argumentDefinition.SymbolValidators)
            };

            return builder;
        }
    }
}
