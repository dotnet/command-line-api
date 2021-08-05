// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a value that can be passed on the command line to a <see cref="ICommand">command</see> or <see cref="IOption">option</see>.
    /// </summary>
    public class Argument : Symbol, IArgument
    {
        private Func<ArgumentResult, object?>? _defaultValueFactory;
        private IArgumentArity? _arity;
        private TryConvertArgument? _convertArguments;
        private Type _argumentType = typeof(string);
        private SuggestionSourceList? _suggestions = null;

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
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name!;
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        [NotNull]
        public IArgumentArity? Arity
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

        /// <summary>
        /// Argument help name
        /// </summary>
        internal string? HelpName { get; set; }

        internal TryConvertArgument? ConvertArguments
        {
            get
            {
                if (_convertArguments is null)
                {
                    if (ArgumentType.CanBeBoundFromScalarValue())
                    {
                        if (Arity.MaximumNumberOfValues == 1 &&
                            ArgumentType == typeof(bool))
                        {
                            _convertArguments = ArgumentConverter.TryConvertBoolArgument;
                        }
                        else
                        {
                            _convertArguments = ArgumentConverter.TryConvertArgument;
                        }
                    }
                }

                return _convertArguments;

          
            }
            set => _convertArguments = value;
        }

        /// <summary>
        /// Gets the list of suggestion sources for the argument.
        /// </summary>
        public SuggestionSourceList Suggestions =>
            _suggestions ??= new SuggestionSourceList
            {
                SuggestionSource.ForType(ArgumentType)
            };

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument token(s) will be converted to.
        /// </summary>
        public virtual Type ArgumentType
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

        internal List<ValidateSymbolResult<ArgumentResult>> Validators { get; } = new();

        /// <summary>
        /// Adds a custom <see cref="ValidateSymbolResult{ArgumentResult}"/> to the argument. Validators can be used
        /// to provide custom errors based on user input.
        /// </summary>
        /// <param name="validate">The delegate to validate the parsed argument.</param>
        public void AddValidator(ValidateSymbolResult<ArgumentResult> validate) => Validators.Add(validate);

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

        internal static Argument None() => new()
        {
            Arity = ArgumentArity.Zero,
            ArgumentType = typeof(bool)
        };

        internal void AddAllowedValues(IEnumerable<string> values)
        {
            if (AllowedValues is null)
            {
                AllowedValues = new HashSet<string>();
            }

            AllowedValues.UnionWith(values);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            return Suggestions
                   .SelectMany(source => source.GetSuggestions(parseResult, textToMatch))
                   .Distinct()
                   .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                   .Containing(textToMatch ?? "");
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Argument)}: {Name}";

        /// <inheritdoc />
        IArgumentArity IArgument.Arity => Arity;

        /// <inheritdoc />
        string IValueDescriptor.ValueName => Name;

        /// <inheritdoc />
        Type IValueDescriptor.ValueType => ArgumentType;
    }
}
