// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a named parameter and a value for that parameter. 
    /// </summary>
    public abstract class CliOption : CliSymbol
    {
        internal AliasSet? _aliases;
        private List<Action<OptionResult>>? _validators;

        private protected CliOption(string name) : base(name)
        {
        }

        private protected CliOption(string name, string[] aliases) : base(name)
        {
            if (aliases is { Length: > 0 }) 
            {
                _aliases = new(aliases);
            }
        }

        /// <summary>
        /// Gets the <see cref="Argument">argument</see> for the option.
        /// </summary>
        internal abstract CliArgument Argument { get; }

        /// <summary>
        /// Gets or sets the name of the Option when displayed in help.
        /// </summary>
        /// <value>
        /// The name of the option when displayed in help.
        /// </value>
        /// <remarks>Useful for localization, as it's not used for actual parsing.</remarks>
        public string? HelpName
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
        /// When set to true, this option will be applied to its immediate parent command or commands and recursively to their subcommands.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// Validators that will be called when the option is matched by the parser.
        /// </summary>
        public List<Action<OptionResult>> Validators => _validators ??= new();

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the option's parsed tokens will be converted to.
        /// </summary>
        public abstract Type ValueType { get; }

        internal bool HasValidators => _validators is not null && _validators.Count > 0;

        /// <summary>
        /// Gets the list of completion sources for the option.
        /// </summary>
        public List<Func<CompletionContext, IEnumerable<CompletionItem>>> CompletionSources => Argument.CompletionSources;

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

        internal virtual bool Greedy
            => Argument.Arity.MinimumNumberOfValues > 0 && Argument.ValueType != typeof(bool);

        /// <summary>
        /// Indicates whether the option is required when its parent command is invoked.
        /// </summary>
        /// <remarks>When an option is required and its parent command is invoked without it, an error results.</remarks>
        public bool Required { get; set; }

        /// <summary>
        /// Gets the unique set of strings that can be used on the command line to specify the Option.
        /// </summary>
        /// <remarks>The collection does not contain the <see cref="CliSymbol.Name"/> of the Option.</remarks>
        public ICollection<string> Aliases => _aliases ??= new();

        /// <summary>
        /// Gets or sets the <see cref="CliAction"/> for the Option. The handler represents the action
        /// that will be performed when the Option is invoked.
        /// </summary>
        public virtual CliAction? Action { get; set; }

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
    }
}
