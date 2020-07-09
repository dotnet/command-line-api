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
        private Func<ArgumentResult, object?>? _defaultValueFactory;
        private IArgumentArity? _arity;
        private TryConvertArgument? _convertArguments;
        private Type _argumentType = typeof(string);
        private List<ISuggestionSource>? _suggestions = null;

        public Argument()
        {
        }

        public Argument(string name) 
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name!;
            }
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        public IArgumentArity Arity
        {
            get
            {
                if (_arity is null)
                {
                    return ArgumentArity.Default(
                        ArgumentType, 
                        this, 
                        Parents.FirstOrDefault());
                }

                return _arity;
            }
            set => _arity = value;
        }

        internal TryConvertArgument? ConvertArguments
        {
            get
            {
                if (_convertArguments == null)
                {
                    if (ArgumentType.CanBeBoundFromScalarValue())
                    {
                        if (Arity.MaximumNumberOfValues == 1 &&
                            ArgumentType == typeof(bool))
                        {
                            _convertArguments = (ArgumentResult symbol, out object? value) =>
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

        public List<ISuggestionSource> Suggestions
        { 
            get
            {
                if (_suggestions is null)
                {
                    _suggestions = new List<ISuggestionSource>
                    {
                        SuggestionSource.ForType(ArgumentType)
                    };
                }

                return _suggestions;
            }
        }

        public Type ArgumentType
        {
            get => _argumentType;
            set => _argumentType = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal List<ValidateSymbol<ArgumentResult>> Validators { get; } = new List<ValidateSymbol<ArgumentResult>>();

        public void AddValidator(ValidateSymbol<ArgumentResult> validator) => Validators.Add(validator);

        public object? GetDefaultValue()
        {
            return GetDefaultValue(new ArgumentResult(this, null));
        }

        internal object? GetDefaultValue(ArgumentResult argumentResult)
        {
            if (_defaultValueFactory is null)
            {
                throw new InvalidOperationException($"Argument \"{Name}\" does not have a default value");
            }

            return _defaultValueFactory.Invoke(argumentResult);
        }

        public void SetDefaultValue(object? value)
        {
            SetDefaultValueFactory(() => value);
        }

        public void SetDefaultValueFactory(Func<object?> getDefaultValue)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(_ => getDefaultValue());
        }
        
        public void SetDefaultValueFactory(Func<ArgumentResult, object?> getDefaultValue)
        {
            _defaultValueFactory = getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue));
        }

        public bool HasDefaultValue => _defaultValueFactory != null;

        internal static Argument None => new Argument { Arity = ArgumentArity.Zero };

        internal void AddAllowedValues(IEnumerable<string> values)
        {
            if (AllowedValues is null)
            {
                AllowedValues = new HashSet<string>();
            }

            AllowedValues.UnionWith(values);
        }

        public override IEnumerable<string?> GetSuggestions(string? textToMatch = null)
        {
            var dynamicSuggestions = Suggestions
                .SelectMany(source => source.GetSuggestions(textToMatch));

            return dynamicSuggestions
                   .Distinct()
                   .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                   .Containing(textToMatch);
        }

        public override string ToString() => $"{nameof(Argument)}: {Name}";

        IArgumentArity IArgument.Arity => Arity;

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => ArgumentType;

        private protected override void ChooseNameForUnnamedArgument(Argument argument)
        {
        }
    }
}
