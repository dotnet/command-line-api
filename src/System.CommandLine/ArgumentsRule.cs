// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ArgumentsRule
    {
        private readonly Func<string> defaultValue;

        public ArgumentsRule(
            ArgumentParser parser,
            Func<string> defaultValue = null,
            ArgumentsRuleHelp help = null,
            IReadOnlyCollection<ValidateSymbol> symbolValidators = null,
            ISuggestionSource suggestionSource = null)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            this.defaultValue = defaultValue;

            Help = help ?? new ArgumentsRuleHelp();

            SuggestionSource = suggestionSource ?? NullSuggestionSource.Instance;

            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }
        }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public Func<string> GetDefaultValue => () => defaultValue?.Invoke();

        public bool HasDefaultValue => defaultValue != null;

        public ArgumentsRuleHelp Help { get; }

        public ArgumentParser Parser { get; }

        public static ArgumentsRule None { get; } = new ArgumentsRule(
            new ArgumentParser(
                ArgumentArity.Zero,
                symbol =>
                {
                    if (symbol.Arguments.Any())
                    {
                        return ArgumentParseResult.Failure(ValidationMessages.NoArgumentsAllowed(symbol.Symbol.ToString()));
                    }

                    return ArgumentParseResult.Success(true);
                }),
            symbolValidators: new ValidateSymbol[] { AcceptNoArguments });

        public ISuggestionSource SuggestionSource { get; }

        private static string AcceptNoArguments(ParsedSymbol o)
        {
            if (!o.Arguments.Any())
            {
                return null;
            }

            return ValidationMessages.Current.NoArgumentsAllowed(o.Symbol.ToString());
        }
    }
}
