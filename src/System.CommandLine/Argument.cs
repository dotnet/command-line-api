// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : IArgument
    {
        private Func<object> _defaultValue;
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<ISuggestionSource> _suggestionSources = new List<ISuggestionSource>();
        private IArgumentArity _arity;
        private HashSet<string> _validValues;
        private ConvertArgument _convertArguments;
        private Symbol _parent;

        public string Name { get; set; }

        public string Description { get; set; }

        public IArgumentArity Arity
        {
            get
            {
                if (_arity == null)
                {
                    if (ArgumentType != null)
                    {
                        return ArgumentArity.Default(ArgumentType, Parent);
                    }
                    else
                    {
                        return ArgumentArity.Zero;
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
                if (_convertArguments == null &&
                    ArgumentType != null)
                {
                    if (ArgumentType.CanBeBoundFromScalarValue())
                    {
                        if (Arity.MaximumNumberOfArguments == 1 &&
                            ArgumentType == typeof(bool))
                        {
                            _convertArguments = symbol =>
                                ArgumentConverter.Parse<bool>(symbol.Arguments.SingleOrDefault() ?? bool.TrueString);
                        }
                        else
                        {
                            _convertArguments = DefaultConvert;
                        }
                    }
                }

                return _convertArguments;

                ArgumentResult DefaultConvert(SymbolResult symbol)
                {
                    switch (Arity.MaximumNumberOfArguments)
                    {
                        case 1:
                            return ArgumentConverter.Parse(
                                ArgumentType,
                                symbol.Arguments.SingleOrDefault());
                        default:
                            return ArgumentConverter.ParseMany(
                                ArgumentType, 
                                symbol.Arguments);
                    }
                }
            }
            set => _convertArguments = value;
        }

        public Type ArgumentType { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public Symbol Parent
        {
            get => _parent;
            internal set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (_parent != null)
                {
                    throw new InvalidOperationException($"{nameof(Parent)} is already set.");
                }

                _parent = value;
            }
        }

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
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

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

        public IEnumerable<string> Suggest(string textToMatch)
        {
            var fixedSuggestions = _suggestions;

            var dynamicSuggestions = _suggestionSources
                .SelectMany(source => source.Suggest(textToMatch));

            var typeSuggestions = SuggestionSource.ForType(ArgumentType)
                                                  .Suggest(textToMatch);

            return fixedSuggestions
                   .Concat(dynamicSuggestions)
                   .Concat(typeSuggestions)
                   .Distinct()
                   .OrderBy(c => c)
                   .Containing(textToMatch);
        }

        private ArgumentResult Parse(SymbolResult symbolResult)
        {
            var failedResult = ArgumentArity.Validate(symbolResult,
                                                      Arity.MinimumNumberOfArguments,
                                                      Arity.MaximumNumberOfArguments);

            if (failedResult != null)
            {
                return failedResult;
            }

            if (symbolResult.UseDefaultValue)
            {
                return ArgumentResult.Success(symbolResult.Symbol.Argument.GetDefaultValue());
            }

            if (ConvertArguments != null)
            {
                return ConvertArguments(symbolResult);
            }

            switch (Arity.MaximumNumberOfArguments)
            {
                case 0:
                    return ArgumentResult.Success((string)null);

                case 1:
                    return ArgumentResult.Success(symbolResult.Arguments.SingleOrDefault());

                default:
                    return ArgumentResult.Success(symbolResult.Arguments);
            }
        }

        internal (ArgumentResult, ParseError) Validate(SymbolResult symbolResult)
        {
            ArgumentResult result = null;

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

                    case FailedArgumentResult general:

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
                                                                  _validValues),
                                symbolResult,
                                canTokenBeRetried: false);
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
                        return new ParseError(errorMessage, symbolResult, false);
                    }
                }

                return null;
            }
        }

        IArgumentArity IArgument.Arity => Arity;

        Type IValueDescriptor.Type => ArgumentType;
    }
}
