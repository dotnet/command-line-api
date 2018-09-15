// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class Argument
    {
        private readonly Func<object> _defaultValue;

        internal Argument(
            ArgumentParser parser,
            Func<object> defaultValue = null,
            HelpDetail help = null,
            IReadOnlyCollection<ValidateSymbol> symbolValidators = null,
            ISuggestionSource suggestionSource = null)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            _defaultValue = defaultValue;

            Help = help ?? new HelpDetail();

            SuggestionSource = suggestionSource ?? NullSuggestionSource.Instance;

            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }
        }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public Func<object> GetDefaultValue => () => _defaultValue?.Invoke();

        public bool HasDefaultValue => _defaultValue != null;

        public HelpDetail Help { get; }

        public bool HasHelp => Help.IsHidden == false;

        internal ArgumentParser Parser { get; }

        internal static Argument None { get; } = new Argument(
            new ArgumentParser(
                System.CommandLine.ArgumentArity.Zero,
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

        public ArgumentArityValidator ArgumentArity => Parser.ArityValidator;

        private static string AcceptNoArguments(SymbolResult symbolResult)
        {
            if (!symbolResult.Arguments.Any())
            {
                return null;
            }

            return symbolResult.ValidationMessages.NoArgumentsAllowed(symbolResult);
        }
    }
}
