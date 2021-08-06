// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a named parameter and a value for that parameter. 
    /// </summary>
    /// <seealso cref="System.CommandLine.IdentifierSymbol" />
    /// <seealso cref="System.CommandLine.IOption" />
    public class Option :
        IdentifierSymbol,
        IOption
    {
        private string? _implicitName;
        private protected readonly HashSet<string> _unprefixedAliases = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class.
        /// </summary>
        /// <param name="name">The name of the option, which can be used to specify it on the command line.</param>
        /// <param name="description">The description of the option shown in help.</param>
        /// <param name="argumentType">The type that the option's argument(s) can be parsed to.</param>
        /// <param name="getDefaultValue">A delegate used to get a default value for the option when it is not specified on the command line.</param>
        /// <param name="arity">The arity of the option.</param>
        public Option(
            string name,
            string? description = null,
            Type? argumentType = null,
            Func<object?>? getDefaultValue = null,
            IArgumentArity? arity = null)
            : this(new[] { name }, description, argumentType, getDefaultValue, arity)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class.
        /// </summary>
        /// <param name="aliases">The set of strings that can be used on the command line to specify the option.</param>
        /// <param name="description">The description of the option shown in help.</param>
        /// <param name="argumentType">The type that the option's argument(s) can be parsed to.</param>
        /// <param name="getDefaultValue">A delegate used to get a default value for the option when it is not specified on the command line.</param>
        /// <param name="arity">The arity of the option.</param>
        public Option(
            string[] aliases,
            string? description = null,
            Type? argumentType = null,
            Func<object?>? getDefaultValue = null,
            IArgumentArity? arity = null)
            : this(aliases, description, CreateArgument(argumentType, getDefaultValue, arity))
        { }

        internal Option(
            string[] aliases,
            string? description,
            Argument? argument)
            : base(description)
        {
            if (aliases is null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (aliases.Length == 0)
            {
                throw new ArgumentException("An option must have at least one alias.", nameof(aliases));
            }

            for (var i = 0; i < aliases.Length; i++)
            {
                var alias = aliases[i];
                AddAlias(alias);
            }

            if (argument != null)
            {
                AddArgumentInner(argument);
            }
        }

        private static Argument? CreateArgument(Type? argumentType, Func<object?>? getDefaultValue, IArgumentArity? arity)
        {
            if (argumentType is null &&
                getDefaultValue is null &&
                arity is null)
            {
                return null;
            }

            var rv = new Argument();
            if (argumentType != null)
            {
                rv.ArgumentType = argumentType;
            }
            if (getDefaultValue != null)
            {
                rv.SetDefaultValueFactory(getDefaultValue);
            }
            if (arity != null)
            {
                rv.Arity = arity;
            }
            return rv;
        }

        internal virtual Argument Argument
        {
            get
            {
                switch (Children.Arguments.Count)
                {
                    case 0:
                        var none = Argument.None();
                        AddSymbol(none);
                        return none;

                    default:
                        DebugAssert.ThrowIf(Children.Arguments.Count > 1, $"Unexpected number of option arguments: {Children.Arguments.Count}");
                        return Children.Arguments[0];
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the argument when displayed in help.
        /// </summary>
        /// <value>
        /// The name of the argument when displayed in help.
        /// </value>
        public string? ArgumentHelpName
        {
            get => Argument.HelpName;
            set
            {
                Argument.HelpName = value;
            }
        }

        /// <summary>
        /// Gets or sets the arity of the option.
        /// </summary>
        public virtual IArgumentArity Arity
        {
            get => Argument.Arity;
            init
            {
                if (value.MaximumNumberOfValues > 0)
                {
                    Argument.ArgumentType = typeof(string);
                }
                
                Argument.Arity = value;
            }
        }

        internal bool DisallowBinding { get; set; }

        /// <inheritdoc />
        public override string Name
        {
            get => base.Name;
            set
            {
                if (!HasAlias(value))
                {
                    _implicitName = null;
                    RemoveAlias(DefaultName);
                }

                base.Name = value;
            }
        }

        internal List<ValidateSymbolResult<OptionResult>> Validators { get; } = new();

        /// <summary>
        /// Adds an alias for the option, which can be used to specify the option on the command line.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        public void AddAlias(string alias) => AddAliasInner(alias);

        private protected override void AddAliasInner(string alias)
        {
            ThrowIfAliasIsInvalid(alias);

            base.AddAliasInner(alias);

            var unprefixedAlias = alias.RemovePrefix();

            _unprefixedAliases.Add(unprefixedAlias!);
        }

        /// <summary>
        /// Adds a validator that will be called when the option is matched by the parser.
        /// </summary>
        /// <param name="validate">A <see cref="ValidateSymbolResult{OptionResult}"/> delegate used to validate the <see cref="OptionResult"/> produced during parsing.</param>
        public void AddValidator(ValidateSymbolResult<OptionResult> validate) => Validators.Add(validate);

        /// <summary>
        /// Indicates whether a given alias exists on the option, regardless of its prefix.
        /// </summary>
        /// <param name="alias">The alias, which can include a prefix.</param>
        /// <returns><see langword="true"/> if the alias exists; otherwise, <see langword="false"/>.</returns>
        public bool HasAliasIgnoringPrefix(string alias) => _unprefixedAliases.Contains(alias.RemovePrefix());

        private protected override void RemoveAlias(string alias)
        {
            _unprefixedAliases.Remove(alias);

            base.RemoveAlias(alias);
        }

        /// <summary>
        /// Sets the default value for the option.
        /// </summary>
        /// <param name="value">The default value for the option.</param>
        public void SetDefaultValue(object? value) => 
            Argument.SetDefaultValue(value);

        /// <summary>
        /// Sets a delegate to invoke when the default value for the option is required.
        /// </summary>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public void SetDefaultValueFactory(Func<object?> getDefaultValue) => 
            Argument.SetDefaultValueFactory(getDefaultValue);

        IArgument IOption.Argument => Argument;

        /// <inheritdoc/>
        public bool AllowMultipleArgumentsPerToken { get; set; }

        /// <summary>
        /// Indicates whether the option is required when its parent command is invoked.
        /// </summary>
        /// <remarks>When an option is required and its parent command is invoked without it, an error results.</remarks>
        public bool IsRequired { get; set; }

        string IValueDescriptor.ValueName => Name;

        /// <summary>
        /// The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.
        /// </summary>
        public Type ValueType => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override string DefaultName =>
            _implicitName ??= Aliases
                              .OrderBy(a => a.Length)
                              .Last()
                              .RemovePrefix();
    }
}
