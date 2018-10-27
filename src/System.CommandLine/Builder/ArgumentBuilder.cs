// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class ArgumentBuilder
    {
        private ArgumentSuggestionSource _suggestionSource;

        internal ArgumentArity ArgumentArity { get; set; }

        internal ConvertArgument ConvertArguments { get; set; }

        internal Func<object> DefaultValue { get; set; }

        internal HelpDetail Help { get; set; }

        internal ArgumentParser Parser { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get; set; } = new List<ValidateSymbol>();

        internal ArgumentSuggestionSource SuggestionSource =>
            _suggestionSource ??
            (_suggestionSource = new ArgumentSuggestionSource());

        internal HashSet<string> ValidTokens { get; } = new HashSet<string>();

        public void AddValidator(ValidateSymbol validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            SymbolValidators.Add(validator);
        }

        internal virtual ArgumentParser BuildArgumentParser()
        {
            var parser = new ArgumentParser(
                ArgumentArity ?? ArgumentArity.Zero,
                ConvertArguments);

            return parser;
        }

        public Argument Build()
        {
            AddTokenValidator();

            var argument = new Argument(
                Parser ?? (Parser = BuildArgumentParser()),
                SymbolValidators,
                _suggestionSource);

            argument.SetDefaultValue(DefaultValue);

            if (Help != null)
            {
                argument.Help.Description = Help.Description;
                argument.Help.Name = Help.Name;
                argument.Help.IsHidden = Help.IsHidden;
            }

            return argument;
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
                        return symbol.ValidationMessages.UnrecognizedArgument(arg, ValidTokens);
                    }
                }

                return null;
            });
        }

        internal static ArgumentBuilder From(Argument argument)
        {
            // TODO: (From) get rid of this method

            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            var suggestionSource = new ArgumentSuggestionSource();
            suggestionSource.AddSuggestionSource(argument.SuggestionSource.Suggest);

            var builder = new ArgumentBuilder
                          {
                              ConvertArguments = argument.Parser.ConvertArguments,
                              DefaultValue = argument.GetDefaultValue,
                              Help = new HelpDetail
                                     {
                                         Name = argument.Help?.Name,
                                         Description = argument.Help?.Description,
                                         IsHidden = argument.Help?.IsHidden ?? HelpDetail.DefaultIsHidden
                                     },
                              Parser = argument.Parser,
                              _suggestionSource = suggestionSource,
                              SymbolValidators = new List<ValidateSymbol>(argument.SymbolValidators)
                          };

            return builder;
        }
    }
}
