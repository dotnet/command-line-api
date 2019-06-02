// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : Symbol, IArgument
    {
        private Func<object> _defaultValue;
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<ISuggestionSource> _suggestionSources = new List<ISuggestionSource>();
        private IArgumentArity _arity;
        private TryConvertArgument _convertArguments;

        internal HashSet<string> AllowedValues { get; private set; }

        public IArgumentArity Arity
        {
            get
            {
                if (_arity == null)
                {
                    if (ArgumentType != null)
                    {
                        return ArgumentArity.Default(ArgumentType, this, Parent);
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

        internal TryConvertArgument ConvertArguments
        {
            get
            {
                if (_convertArguments == null &&
                    ArgumentType != null)
                {
                    if (ArgumentType.CanBeBoundFromScalarValue())
                    {
                        if (Arity.MaximumNumberOfValues == 1 &&
                            ArgumentType == typeof(bool))
                        {
                            _convertArguments = (SymbolResult symbol, out object value) =>
                            {
                                value = ArgumentConverter.ConvertObject(
                                    this,
                                    typeof(bool),
                                    symbol.Tokens.SingleOrDefault()?.Value ?? bool.TrueString);

                                return value is SuccessfulArgumentResult;
                            };
                        }
                        else
                        {
                            _convertArguments = DefaultConvert;
                        }
                    }
                }

                return _convertArguments;

                bool DefaultConvert(SymbolResult symbol, out object value)
                {
                    switch (Arity.MaximumNumberOfValues)
                    {
                        case 1:
                            value = ArgumentConverter.ConvertObject(
                                this,
                                ArgumentType,
                                symbol.Arguments.SingleOrDefault());
                            break;

                        default:
                            value = ArgumentConverter.ConvertStrings(
                                this,
                                ArgumentType,
                                symbol.Arguments);
                            break;
                    }

                    return value is SuccessfulArgumentResult;
                }
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
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            AddSuggestionSource(new AnonymousSuggestionSource(suggest));
        }

        internal void AddAllowedValues(IEnumerable<string> values)
        {
            if (AllowedValues == null)
            {
                AllowedValues = new HashSet<string>();
            }

            AllowedValues.UnionWith(values);
        }

        public override IEnumerable<string> Suggest(string textToMatch)
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

        public override string ToString() => $"{nameof(Argument)}: {Name}";

        IArgumentArity IArgument.Arity => Arity;

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.Type => ArgumentType;
    }
}
