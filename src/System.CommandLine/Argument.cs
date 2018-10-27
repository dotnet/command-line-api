// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : IArgument
    {
        private readonly Func<object> _defaultValue;
        private  HelpDetail _helpDetail;

        internal Argument(
            ArgumentParser parser,
            Func<object> defaultValue = null,
            IReadOnlyCollection<ValidateSymbol> symbolValidators = null,
            ISuggestionSource suggestionSource = null)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            _defaultValue = defaultValue;

            SuggestionSource = suggestionSource ?? NullSuggestionSource.Instance;

            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }
        }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public object GetDefaultValue() => _defaultValue?.Invoke();

        public bool HasDefaultValue => _defaultValue != null;

        public HelpDetail Help => _helpDetail ?? (_helpDetail = new HelpDetail());

        internal ArgumentParser Parser { get; }

        internal static Argument None { get; } = new Argument(
            new ArgumentParser(
                ArgumentArity.Zero,
                symbol =>
                {
                    if (symbol.Arguments.Any())
                    {
                        return ArgumentParseResult.Failure(symbol.ValidationMessages.NoArgumentsAllowed(symbol));
                    }

                    return SuccessfulArgumentParseResult.Empty;
                }),
            symbolValidators: new ValidateSymbol[] { AcceptNoArguments });

        public ISuggestionSource SuggestionSource { get; }

        public ArgumentArity Arity => Parser.Arity;

        internal (ArgumentParseResult, ParseError) Validate(SymbolResult symbolResult)
        {
            ParseError error = null;
            ArgumentParseResult result = null;

            foreach (var symbolValidator in SymbolValidators)
            {
                var errorMessage = symbolValidator(symbolResult);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    error = new ParseError(errorMessage, symbolResult);
                }
            }

            if (error == null)
            {
                result = Parser.Parse(symbolResult);

                var canTokenBeRetried =
                    symbolResult.Symbol is ICommand ||
                    Arity.MinimumNumberOfArguments == 0;

                switch (result)
                {
                    case FailedArgumentArityResult arityFailure:

                        error = new ParseError(arityFailure.ErrorMessage,
                                               symbolResult,
                                               canTokenBeRetried);
                        break;

                    case FailedArgumentTypeConversionResult conversionFailure:

                        error = new ParseError(conversionFailure.ErrorMessage,
                                               symbolResult,
                                               canTokenBeRetried);
                        break;

                    case FailedArgumentParseResult general:

                        error = new ParseError(general.ErrorMessage,
                                               symbolResult,
                                               false);

                        break;
                }
            }

            return (result, error);
        }

        private static string AcceptNoArguments(SymbolResult symbolResult)
        {
            if (!symbolResult.Arguments.Any())
            {
                return null;
            }

            return symbolResult.ValidationMessages.NoArgumentsAllowed(symbolResult);
        }

        IHelpDetail IArgument.Help => Help;
    }
}
