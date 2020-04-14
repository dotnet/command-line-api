// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    public class Argument : Symbol, IArgument
    {
        private Func<ArgumentResult, object>? _defaultValueFactory;
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<ISuggestionSource> _suggestionSources = new List<ISuggestionSource>();
        private IArgumentArity? _arity;
        private TryConvertArgument? _convertArguments;

        public Argument()
        {
        }

        public Argument(string? name) 
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        public IArgumentArity Arity
        {
            get
            {
                if (_arity == null)
                {
                    if (ArgumentType != null)
                    {
                        return ArgumentArity.Default(ArgumentType, this, Parents.FirstOrDefault());
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

        internal TryConvertArgument? ConvertArguments
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
                            _convertArguments = (ArgumentResult symbol, out object value) =>
                            {
                                value = ArgumentConverter.ConvertObject(
                                    this,
                                    typeof(bool),
                                    symbol.Tokens.SingleOrDefault()?.Value ?? bool.TrueString);

                                return value is SuccessfulArgumentConversionResult;
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
                                symbol.Tokens.Select(t => t.Value).SingleOrDefault());
                            break;

                        default:
                            value = ArgumentConverter.ConvertStrings(
                                this,
                                ArgumentType,
                                symbol.Tokens.Select(t => t.Value).ToArray());
                            break;
                    }

                    return value is SuccessfulArgumentConversionResult;
                }
            }
            set => _convertArguments = value;
        }

        public Type? ArgumentType { get; set; }

        internal List<ValidateSymbol<ArgumentResult>> Validators { get; } = new List<ValidateSymbol<ArgumentResult>>();

        public void AddValidator(ValidateSymbol<ArgumentResult> validator) => Validators.Add(validator);

        public object GetDefaultValue()
        {
            return GetDefaultValue(new ArgumentResult(this, null));
        }

        internal object GetDefaultValue(ArgumentResult argumentResult)
        {
            if (_defaultValueFactory is null)
            {
                throw new InvalidOperationException($"Argument \"{Name}\" does not have a default value");
            }

            return _defaultValueFactory.Invoke(argumentResult);
        }

        public void SetDefaultValue(object value)
        {
            SetDefaultValueFactory(() => value);
        }

        public void SetDefaultValueFactory(Func<object> getDefaultValue)
        {
            if (getDefaultValue == null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(_ => getDefaultValue());
        }
        
        public void SetDefaultValueFactory(Func<ArgumentResult, object> getDefaultValue)
        {
            _defaultValueFactory = getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue));
        }

        public bool HasDefaultValue => _defaultValueFactory != null;

        internal static Argument None => new Argument { Arity = ArgumentArity.Zero };

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

        public override IEnumerable<string> GetSuggestions(string? textToMatch = null)
        {
            var fixedSuggestions = _suggestions;

            var dynamicSuggestions = _suggestionSources
                .SelectMany(source => source.GetSuggestions(textToMatch));

            var typeSuggestions = SuggestionSource.ForType(ArgumentType)
                                                  .GetSuggestions(textToMatch);

            return fixedSuggestions
                   .Concat(dynamicSuggestions)
                   .Concat(typeSuggestions)
                   .Distinct()
                   .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                   .Containing(textToMatch);
        }

        public override string ToString() => $"{nameof(Argument)}: {Name}";

        IArgumentArity IArgument.Arity => Arity;

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => ArgumentType;
    }
}
