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
        private List<Action<OptionResult>>? _validators;

        private protected Option(string name, string? description) : base(description)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name.RemovePrefix();

            AddAlias(name);
        }

        private protected Option(string[] aliases, string? description) : base(description)
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
        }

        /// <summary>
        /// Gets the <see cref="Argument">argument</see> for the option.
        /// </summary>
        internal abstract Argument Argument { get; }

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
        public ArgumentArity Arity
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

        internal List<Action<OptionResult>> Validators => _validators ??= new();

        internal bool HasValidators => _validators is not null && _validators.Count > 0;

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
            => Argument is not null && Argument.Arity.MinimumNumberOfValues > 0 && Argument.ValueType != typeof(bool);

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

        private protected override string DefaultName => GetLongestAlias(true);

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            if (Argument is null)
            {
                return Array.Empty<CompletionItem>();
            }

            List<CompletionItem>? completions = null;

            foreach (var completion in Argument.GetCompletions(context))
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

        /// <summary>
        /// Parses a command line string value using the option.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="commandLine">A command line string to parse, which can include spaces and quotes equivalent to what can be entered into a terminal.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public ParseResult Parse(string commandLine) =>
            this.GetOrCreateDefaultSimpleParser().Parse(commandLine);

        /// <summary>
        /// Parses a command line string value using the option.
        /// </summary>
        /// <param name="args">The string options to parse.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public ParseResult Parse(string[] args) =>
            this.GetOrCreateDefaultSimpleParser().Parse(args);
    }
}
