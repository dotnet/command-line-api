// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : IArgument, ISuggestionSource
    {
        private Func<object> _defaultValue;
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<ISuggestionSource> _suggestionSources = new List<ISuggestionSource>();
        private IArgumentArity _arity;
        private HashSet<string> _validValues;
        private ConvertArgument _convertArguments;

        public Argument()
        {
        }

        public Argument(IReadOnlyCollection<ValidateSymbol> symbolValidators)
        {
            if (symbolValidators == null)
            {
                throw new ArgumentNullException(nameof(symbolValidators));
            }

            SymbolValidators.AddRange(symbolValidators);
        }

        public string Name {get;set;}
        public string Description {get;set;}
        public bool IsHidden {get;set;}
        public IArgumentArity Arity
        {
            get
            {
                if (_arity == null)
                {
                    if (ArgumentType != null)
                    {
                        _arity = ArgumentArity.DefaultForType(ArgumentType);
                    }
                    else
                    {
                        _arity = ArgumentArity.Zero;
                    }
                }

                return _arity;
            }
            set => _arity = value;
        }

        internal ConvertArgument ConvertArguments
        {
            get
            {
                if (_convertArguments == null)
                {
                    if (ArgumentType != null)
                    {
                        if (Arity.MaximumNumberOfArguments == 1 &&
                            ArgumentType == typeof(bool))
                        {
                            _convertArguments = symbol =>
                                ArgumentConverter.Parse<bool>(symbol.Arguments.SingleOrDefault() ?? bool.TrueString);
                        }
                        else
                        {
                            _convertArguments = ArgumentConverter.DefaultConvertArgument(ArgumentType);
                        }
                    }
                }

                return _convertArguments;
            }
            set => _convertArguments = value;
        }

        public Type ArgumentType { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public void AddValidator(ValidateSymbol validator) => SymbolValidators.Add(validator);

        public object GetDefaultValue() => _defaultValue?.Invoke();

        public void SetDefaultValue(object value) => SetDefaultValue(() => value);

        public void SetDefaultValue(Func<object> value) => _defaultValue = value;

        public bool HasDefaultValue => _defaultValue != null;

        public static Argument None => new Argument { Arity = ArgumentArity.Zero };

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

            var typeSuggestions = SuggestionSource.ForType(ArgumentType)
                                                  .Suggest(parseResult, position);

            return fixedSuggestions
                   .Concat(dynamicSuggestions)
                   .Concat(typeSuggestions)
                   .Distinct()
                   .OrderBy(c => c)
                   .Containing(parseResult.TextToMatch());
        }

        internal ArgumentParseResult Parse(SymbolResult symbolResult)
        {
            var failedResult = ArgumentArity.Validate(symbolResult,
                                                      Arity.MinimumNumberOfArguments,
                                                      Arity.MaximumNumberOfArguments);

            if (failedResult != null)
            {
                return failedResult;
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
            ArgumentParseResult result = null;

            var error = UnrecognizedArgumentError() ??
                        CustomError();

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

            ParseError UnrecognizedArgumentError()
            {
                if (_validValues?.Count > 0 &&
                    symbolResult.Arguments.Count > 0)
                {
                    foreach (var arg in symbolResult.Arguments)
                    {
                        if (!_validValues.Contains(arg))
                        {
                            return new ParseError(
                                symbolResult.ValidationMessages
                                            .UnrecognizedArgument(arg,
                                                                  _validValues));
                        }
                    }
                }

                return null;
            }

            ParseError CustomError()
            {
                foreach (var symbolValidator in SymbolValidators)
                {
                    var errorMessage = symbolValidator(symbolResult);

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        return new ParseError(errorMessage, symbolResult);
                    }
                }

                return null;
            }
        }

        IArgumentArity IArgument.Arity => Arity;
    }
}
