// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ArgumentDefinition
    {
        private readonly Func<string> _defaultValue;

        public ArgumentDefinition(
            ArgumentParser parser,
            Func<string> defaultValue = null,
            ArgumentsRuleHelp help = null,
            IReadOnlyCollection<ValidateSymbol> symbolValidators = null,
            ISuggestionSource suggestionSource = null)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            _defaultValue = defaultValue;

            Help = help ?? new ArgumentsRuleHelp();

            SuggestionSource = suggestionSource ?? NullSuggestionSource.Instance;

            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }
        }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public Func<string> GetDefaultValue => () => _defaultValue?.Invoke();

        public bool HasDefaultValue => _defaultValue != null;

        public ArgumentsRuleHelp Help { get; }

        public ArgumentParser Parser { get; }

        public static ArgumentDefinition None { get; } = new ArgumentDefinition(
            new ArgumentParser(
                ArgumentArity.Zero,
                symbol =>
                {
                    if (symbol.Arguments.Any())
                    {
                        return ArgumentParseResult.Failure(ValidationMessages.NoArgumentsAllowed(symbol.SymbolDefinition.ToString()));
                    }

                    return ArgumentParseResult.Success(true);
                }),
            help: new ArgumentsRuleHelp(null, null, true),
            symbolValidators: new ValidateSymbol[] { AcceptNoArguments });

        public ISuggestionSource SuggestionSource { get; }

        private static string AcceptNoArguments(Symbol o)
        {
            if (!o.Arguments.Any())
            {
                return null;
            }

            return ValidationMessages.Current.NoArgumentsAllowed(o.SymbolDefinition.ToString());
        }
    }
}
