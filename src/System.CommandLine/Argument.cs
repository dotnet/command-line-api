// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Represents a value passed to an <see cref="Option"/> or <see cref="Command"/>.
    /// </summary>
    public class Argument : Symbol, IArgument
    {
        private Func<ArgumentResult, object?>? _defaultValueFactory;
        private IArgumentArity? _arity;
        private TryConvertArgument? _convertArguments;
        private Type _argumentType = typeof(string);
        private List<ISuggestionSource>? _suggestions = null;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        public Argument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        public Argument(string name) 
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name!;
            }
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        public IArgumentArity Arity
        {
            get
            {
                if (_arity is null)
                {
                    return ArgumentArity.Default(
                        ArgumentType, 
                        this, 
                        Parents);
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

                bool DefaultConvert(ArgumentResult argumentResult, out object value)
                {
                    switch (Arity.MaximumNumberOfValues)
                    {
                        case 1:
                            value = ArgumentConverter.ConvertObject(
                                this,
                                ArgumentType,
                                argumentResult.Tokens.Select(t => t.Value).SingleOrDefault());
                            break;

                        default:
                            value = ArgumentConverter.ConvertStrings(
                                this,
                                ArgumentType,
                                argumentResult.Tokens.Select(t => t.Value).ToArray(),
                                argumentResult);
                            break;
                    }

                    return value is SuccessfulArgumentConversionResult;
                }
            }
            set => _convertArguments = value;
        }

        /// <summary>
        /// Gets the list of suggestion sources for the argument.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the <see cref="System.Type" /> that the argument will be converted to.
        /// </summary>
        public Type ArgumentType
        {
            get => _argumentType;
            set => _argumentType = value ?? throw new ArgumentNullException(nameof(value));
        }

        private protected override string DefaultName
        {
            get
            {
                if (Parents.Count == 1)
                {
                    switch (Parents[0])
                    {
                        case Option option:
                            return option.Name;
                        case Command _:
                            return ArgumentType.Name.ToLowerInvariant();
                    }
                }

                return "";
            }
        }

        internal List<ValidateSymbol<ArgumentResult>> Validators { get; } = new List<ValidateSymbol<ArgumentResult>>();

        /// <summary>
        /// Adds a custom <see cref="ValidateSymbol{T}(ArgumentResult)"/> to the argument. Validators can be used
        /// to provide custom errors based on user input.
        /// </summary>
        /// <param name="validate">The delegate to validate the parsed argument.</param>
        public void AddValidator(ValidateSymbol<ArgumentResult> validate) => Validators.Add(validate);

        /// <summary>
        /// Gets the default value for the argument.
        /// </summary>
        /// <returns>Returns the default value for the argument, if defined. Null otherwise.</returns>
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

        /// <summary>
        /// Sets the default value for the argument.
        /// </summary>
        /// <param name="value">The default value for the argument.</param>
        public void SetDefaultValue(object? value)
        {
            SetDefaultValueFactory(() => value);
        }

        /// <summary>
        /// Sets a delegate to invoke when the default value for the argument is required.
        /// </summary>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public void SetDefaultValueFactory(Func<object?> getDefaultValue)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(_ => getDefaultValue());
        }
        
        /// <summary>
        /// Sets a delegate to invoke when the default value for the argument is required.
        /// </summary>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <remarks>In this overload, the <see cref="ArgumentResult"/> is provided to the delegate.</remarks>
        public void SetDefaultValueFactory(Func<ArgumentResult, object?> getDefaultValue)
        {
            _defaultValueFactory = getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue));
        }

        /// <summary>
        /// Specifies if a default value is defined for the argument.
        /// </summary>
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

        /// <inheritdoc />
        public override IEnumerable<string?> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            var dynamicSuggestions = Suggestions
                .SelectMany(source => source.GetSuggestions(parseResult, textToMatch));

            return dynamicSuggestions
                   .Distinct()
                   .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                   .Containing(textToMatch);
        }

        public override string ToString() => $"{nameof(Argument)}: {Name}";

        /// <inheritdoc />
        IArgumentArity IArgument.Arity => Arity;

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => ArgumentType;
    }
}
