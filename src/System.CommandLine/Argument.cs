// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : IArgument, ISuggestionSource
    {
        private Func<object> _defaultValue;
        private HelpDetail _helpDetail;
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<ISuggestionSource> _suggestionSources = new List<ISuggestionSource>();
        private ArgumentArity _arity;
        private HashSet<string> _validValues;

        internal Argument(IReadOnlyCollection<ValidateSymbol> symbolValidators = null)
        {
            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }
        }

        internal ConvertArgument ConvertArguments { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public object GetDefaultValue() => _defaultValue?.Invoke();

        public void SetDefaultValue(object value) => _defaultValue = () => value;

        public void SetDefaultValue(Func<object> value) => _defaultValue = value;

        public bool HasDefaultValue => _defaultValue != null;

        public HelpDetail Help => _helpDetail ?? (_helpDetail = new HelpDetail());

        internal static Argument None { get; } =
            new Argument(
                symbolValidators: new ValidateSymbol[] { AcceptNoArguments })
            {
                ConvertArguments = symbol =>
                {
                    if (symbol.Arguments.Any())
                    {
                        return ArgumentParseResult.Failure(symbol.ValidationMessages.NoArgumentsAllowed(symbol));
                    }

                    return SuccessfulArgumentParseResult.Empty;
                }
            };

        public void AddSuggestions(IReadOnlyCollection<string> suggestions)
        {
            if (suggestions == null)
            {
                throw new ArgumentNullException(nameof(suggestions));
            }

            _suggestions.AddRange(suggestions);
        }

        public void AddSuggestionSource(ISuggestionSource suggest)
        {
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            _suggestionSources.Add(suggest);
        }

        public void AddSuggestionSource(Suggest suggest)
        {
            AddSuggestionSource(new AnonymousSuggestionSource(suggest));
        }

        internal void AddValidValues(IEnumerable<string> values)
        {
            if (_validValues == null)
            {
                _validValues = new HashSet<string>();
            }

            _validValues.UnionWith(values);
        }

        public IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            var fixedSuggestions = _suggestions;

            var dynamicSuggestions = _suggestionSources
                .SelectMany(source => source.Suggest(parseResult, position));

            return fixedSuggestions
                   .Concat(dynamicSuggestions)
                   .Distinct()
                   .OrderBy(c => c)
                   .Containing(parseResult.TextToMatch());
        }

        public ArgumentArity Arity
        {
            get => _arity ?? (_arity = ArgumentArity.Zero);
            set => _arity = value;
        }

        internal ArgumentParseResult Parse(SymbolResult symbolResult)
        {
            var error = Arity.Validate(symbolResult);

            if (error != null)
            {
                return error;
            }

            if (ConvertArguments != null)
            {
                return ConvertArguments(symbolResult);
            }

            switch (Arity.MaximumNumberOfArguments)
            {
                case 0:
                    return ArgumentParseResult.Success((string)null);

                case 1:
                    return ArgumentParseResult.Success(symbolResult.Arguments.SingleOrDefault());

                default:
                    return ArgumentParseResult.Success(symbolResult.Arguments);
            }
        }

        internal (ArgumentParseResult, ParseError) Validate(SymbolResult symbolResult)
        {
            ParseError error = null;
            ArgumentParseResult result = null;

            if (_validValues?.Count > 0 &&
                symbolResult.Arguments.Count > 0)
            {
                foreach (var arg in symbolResult.Arguments)
                {
                    if (!_validValues.Any(value => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        error = new ParseError(
                            symbolResult.ValidationMessages.UnrecognizedArgument(arg, _validValues));
                    }
                }
            }

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
                result = Parse(symbolResult);

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
