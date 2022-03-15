// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a named parameter and a value for that parameter. 
    /// </summary>
    /// <seealso cref="IdentifierSymbol" />
    public abstract class Option : IdentifierSymbol, IValueDescriptor
    {
        private string? _name;
        private List<ValidateSymbolResult<OptionResult>>? _validators;
        private readonly Argument _argument;

        internal Option(
            string name,
            string? description,
            Argument argument)
            : base(description)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name.RemovePrefix();

            AddAlias(name);

            argument.AddParent(this);
            _argument = argument;
        }

        internal Option(
            string[] aliases,
            string? description,
            Argument argument)
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
                AddAlias(aliases[i]);
            }

            argument.AddParent(this);
            _argument = argument;
        }

        /// <summary>
        /// Gets the <see cref="Argument">argument</see> for the option.
        /// </summary>
        internal virtual Argument Argument => _argument;

        /// <summary>
        /// Gets or sets the name of the argument when displayed in help.
        /// </summary>
        /// <value>
        /// The name of the argument when displayed in help.
        /// </value>
        public string? ArgumentHelpName
        {
            get => Argument.HelpName;
            set => Argument.HelpName = value;
        }

        /// <summary>
        /// Gets or sets the arity of the option.
        /// </summary>
        public virtual ArgumentArity Arity
        {
            get => Argument.Arity;
            set => Argument.Arity = value;
        }

        /// <summary>
        /// Global options are applied to the command and recursively to subcommands.
        /// They do not apply to parent commands.
        /// </summary>
        internal bool IsGlobal { get; set; }

        internal bool DisallowBinding { get; init; }

        /// <inheritdoc />
        public override string Name
        {
            set
            {
                if (!HasAlias(value))
                {
                    _name = null;
                    RemoveAlias(DefaultName);
                }

                base.Name = value;
            }
        }

        internal List<ValidateSymbolResult<OptionResult>> Validators => _validators ??= new();

        internal bool HasValidators => _validators is not null && _validators.Count > 0;

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
        public bool HasAliasIgnoringPrefix(string alias)
        {
            ReadOnlySpan<char> rawAlias = alias.AsSpan(alias.GetPrefixLength());

            foreach (string existingAlias in _aliases)
            {
                if (MemoryExtensions.Equals(existingAlias.AsSpan(existingAlias.GetPrefixLength()), rawAlias, StringComparison.CurrentCulture))
                {
                    return true;
                }
            }

            return false;
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

        /// <summary>
        /// Gets a value that indicates whether multiple argument tokens are allowed for each option identifier token.
        /// </summary>
        /// <example>
        /// If set to <see langword="true"/>, the following command line is valid for passing multiple arguments:
        /// <code>
        /// > --opt 1 2 3
        /// </code>
        /// The following is equivalent and is always valid:
        /// <code>
        /// > --opt 1 --opt 2 --opt 3
        /// </code>
        /// </example>
        public bool AllowMultipleArgumentsPerToken { get; set; }

        internal virtual bool IsGreedy
            => _argument is not null && _argument.Arity.MinimumNumberOfValues > 0 && _argument.ValueType != typeof(bool);

        /// <summary>
        /// Indicates whether the option is required when its parent command is invoked.
        /// </summary>
        /// <remarks>When an option is required and its parent command is invoked without it, an error results.</remarks>
        public bool IsRequired { get; set; }

        string IValueDescriptor.ValueName => Name;

        /// <summary>
        /// The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.
        /// </summary>
        public Type ValueType => Argument.ValueType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override string DefaultName => _name ??= GetLongestAlias();
        
        private string GetLongestAlias()
        {
            string max = "";
            foreach (string alias in _aliases)
            {
                if (alias.Length > max.Length)
                {
                    max = alias;
                }
            }
            return max.RemovePrefix();
        }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            if (_argument is null)
            {
                return Array.Empty<CompletionItem>();
            }

            List<CompletionItem>? completions = null;

            foreach (var completion in _argument.GetCompletions(context))
            {
                if (completion.Label.ContainsCaseInsensitive(context.WordToComplete))
                {
                    (completions ??= new List<CompletionItem>()).Add(completion);
                }
            }

            if (completions is null)
            {
                return Array.Empty<CompletionItem>();
            }

            return completions
                   .OrderBy(item => item.SortText.IndexOfCaseInsensitive(context.WordToComplete))
                   .ThenBy(symbol => symbol.Label, StringComparer.OrdinalIgnoreCase);
        }
    }
}
