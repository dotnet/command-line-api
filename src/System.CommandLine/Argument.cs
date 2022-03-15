// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a value that can be passed on the command line to a <see cref="Command">command</see> or <see cref="Option">option</see>.
    /// </summary>
    public abstract class Argument : Symbol, IValueDescriptor
    {
        private Func<ArgumentResult, object?>? _defaultValueFactory;
        private ArgumentArity _arity;
        private TryConvertArgument? _convertArguments;
        private CompletionSourceList? _completions = null;
        private List<ValidateSymbolResult<ArgumentResult>>? _validators = null;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        protected Argument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        protected Argument(string? name = null, string? description = null)
        {
            Name = name!;
            Description = description;
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        public ArgumentArity Arity
        {
            get
            {
                if (!_arity.IsNonDefault)
                {
                    _arity = ArgumentArity.Default(
                        ValueType, 
                        this, 
                        FirstParent);
                }

                return _arity;
            }
            set => _arity = value;
        }

        /// <summary>
        /// The name used in help output to describe the argument. 
        /// </summary>
        public string? HelpName { get; set; }

        internal TryConvertArgument? ConvertArguments
        {
            get => _convertArguments ??= ArgumentConverter.GetConverter(this);
            init => _convertArguments = value;
        }

        /// <summary>
        /// Gets the list of completion sources for the argument.
        /// </summary>
        public CompletionSourceList Completions =>
            _completions ??= new CompletionSourceList
            {
                CompletionSource.ForType(ValueType)
            };

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument token(s) will be converted to.
        /// </summary>
        public abstract Type ValueType { get; }

        private protected override string DefaultName
        {
            get
            {
                if (FirstParent is not null && FirstParent.Next is null)
                {
                    switch (FirstParent.Symbol)
                    {
                        case Option option:
                            return option.Name;
                        case Command _:
                            return ValueType.Name.ToLowerInvariant();
                    }
                }

                return "";
            }
        }

        internal List<ValidateSymbolResult<ArgumentResult>> Validators => _validators ??= new ();

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
        public bool HasDefaultValue => _defaultValueFactory is not null;

        internal virtual bool HasCustomParser => false;

        internal static Argument None() => new Argument<bool>
        {
            Arity = ArgumentArity.Zero
        };

        internal void AddAllowedValues(IReadOnlyList<string> values)
        {
            if (AllowedValues is null)
            {
                AllowedValues = new HashSet<string>();
            }

            AllowedValues.UnionWith(values);
        }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            return Completions
                   .SelectMany(source => source.GetCompletions(context))
                   .Distinct()
                   .OrderBy(c => c.SortText, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Argument)}: {Name}";

        /// <inheritdoc />
        string IValueDescriptor.ValueName => Name;
    }
}
