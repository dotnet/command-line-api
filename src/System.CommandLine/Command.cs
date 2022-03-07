// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Represents a specific action that the application performs.
    /// </summary>
    /// <remarks>
    /// Use the Command object for actions that correspond to a specific string (the command name). See
    /// <see cref="RootCommand"/> for simple applications that only have one action. For example, <c>dotnet run</c>
    /// uses <c>run</c> as the command.
    /// </remarks>
    public class Command : IdentifierSymbol, IEnumerable<Symbol>
    {
        private List<Argument>? _arguments;
        private List<Option>? _options;
        private List<Command>? _subcommands;
        private List<ValidateSymbolResult<CommandResult>>? _validators;

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command, shown in help.</param>
        public Command(string name, string? description = null) : base(name, description)
        {
        }

        /// <summary>
        /// Gets the child symbols.
        /// </summary>
        public IEnumerable<Symbol> Children
        {
            get
            {
                foreach (var command in Subcommands)
                    yield return command;

                foreach (var option in Options)
                    yield return option;

                foreach (var argument in Arguments)
                    yield return argument;
            }
        }

        /// <summary>
        /// Represents all of the arguments for the command.
        /// </summary>
        public IReadOnlyList<Argument> Arguments => _arguments is not null ? _arguments : Array.Empty<Argument>();

        internal bool HasArguments => _arguments is not null;

        /// <summary>
        /// Represents all of the options for the command, including global options that have been applied to any of the command's ancestors.
        /// </summary>
        public IReadOnlyList<Option> Options => _options is not null ? _options : Array.Empty<Option>();

        /// <summary>
        /// Represents all of the subcommands for the command.
        /// </summary>
        public IReadOnlyList<Command> Subcommands => _subcommands is not null ? _subcommands : Array.Empty<Command>();

        internal IReadOnlyList<ValidateSymbolResult<CommandResult>> Validators
            => _validators is not null ? _validators : Array.Empty<ValidateSymbolResult<CommandResult>>();

        internal bool HasValidators => _validators is not null; // initialized by Add method, so when it's not null the Count is always > 0

        /// <summary>
        /// Adds an <see cref="Argument"/> to the command.
        /// </summary>
        /// <param name="argument">The argument to add to the command.</param>
        public void AddArgument(Argument argument)
        {
            argument.AddParent(this);
            (_arguments ??= new()).Add(argument);
        }

        /// <summary>
        /// Adds a subcommand to the command.
        /// </summary>
        /// <param name="command">The subcommand to add to the command.</param>
        /// <remarks>Commands can be nested to an arbitrary depth.</remarks>
        public void AddCommand(Command command)
        {
            command.AddParent(this);
            (_subcommands ??= new()).Add(command);
        }

        /// <summary>
        /// Adds an <see cref="Option"/> to the command.
        /// </summary>
        /// <param name="option">The option to add to the command.</param>
        public void AddOption(Option option)
        {
            option.AddParent(this);
            (_options ??= new()).Add(option);
        }

        /// <summary>
        /// Adds a global <see cref="Option"/> to the command.
        /// </summary>
        /// <param name="option">The global option to add to the command.</param>
        /// <remarks>Global options are applied to the command and recursively to subcommands. They do not apply to
        /// parent commands.</remarks>
        public void AddGlobalOption(Option option)
        {
            option.IsGlobal = true;
            AddOption(option);
        }
        /// <summary>
        /// Adds an <see cref="Option"/> to the command.
        /// </summary>
        /// <param name="option">The option to add to the command.</param>
        public void Add(Option option) => AddOption(option);

        /// <summary>
        /// Adds an <see cref="Argument"/> to the command.
        /// </summary>
        /// <param name="argument">The argument to add to the command.</param>
        public void Add(Argument argument) => AddArgument(argument);

        /// <summary>
        /// Adds a subcommand to the command.
        /// </summary>
        /// <param name="command">The subcommand to add to the command.</param>
        /// <remarks>Commands can be nested to an arbitrary depth.</remarks>
        public void Add(Command command) => AddCommand(command);

        private protected override string DefaultName => throw new NotImplementedException();

        /// <summary>
        /// Adds a custom validator to the command. Validators can be used
        /// to create custom validation logic.
        /// </summary>
        /// <param name="validate">The delegate to validate the symbols during parsing.</param>
        public void AddValidator(ValidateSymbolResult<CommandResult> validate) => (_validators ??= new()).Add(validate);

        /// <summary>
        /// Gets or sets a value that indicates whether unmatched tokens should be treated as errors. For example,
        /// if set to <see langword="true"/> and an extra command or argument is provided, validation will fail.
        /// </summary>
        public bool TreatUnmatchedTokensAsErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="ICommandHandler"/> for the command. The handler represents the action
        /// that will be performed when the command is invoked.
        /// </summary>
        /// <remarks>
        /// <para>Use one of the <see cref="Handler.SetHandler(Command, Action)" /> overloads to construct a handler.</para>
        /// <para>If the handler is not specified, parser errors will be generated for command line input that
        /// invokes this command.</para></remarks>
        public ICommandHandler? Handler { get; set; }

        /// <summary>
        /// Represents all of the symbols for the command.
        /// </summary>
        public IEnumerator<Symbol> GetEnumerator() => Children.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal Parser? ImplicitInvocationParser { get; set; }

        internal Parser? ImplicitSimpleParser { get; set; }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            var completions = new List<CompletionItem>();

            if (context.WordToComplete is { } textToMatch)
            {
                var commands = Subcommands;
                for (int i = 0; i < commands.Count; i++)
                {
                    AddCompletionsFor(commands[i]);
                }

                var options = Options;
                for (int i = 0; i < options.Count; i++)
                {
                    AddCompletionsFor(options[i]);
                }

                var arguments = Arguments;
                for (int i = 0; i < arguments.Count; i++)
                {
                    var argument = arguments[i];
                    foreach (var completion in argument.GetCompletions(context))
                    {
                        if (completion.Label.ContainsCaseInsensitive(textToMatch))
                        {
                            completions.Add(completion);
                        }
                    }
                }

                foreach (var parent in Parents.FlattenBreadthFirst(p => p.Parents))
                {
                    if (parent is Command parentCommand)
                    {
                        for (var i = 0; i < parentCommand.Options.Count; i++)
                        {
                            var option = parentCommand.Options[i];

                            if (option.IsGlobal)
                            {
                                AddCompletionsFor(option);
                            }
                        }
                    }
                }
            }

            return completions
                   .OrderBy(item => item.SortText.IndexOfCaseInsensitive(context.WordToComplete))
                   .ThenBy(symbol => symbol.Label, StringComparer.OrdinalIgnoreCase);

            void AddCompletionsFor(IdentifierSymbol identifier)
            {
                if (!identifier.IsHidden)
                {
                    foreach (var alias in identifier.Aliases)
                    {
                        if (alias is { } &&
                            alias.ContainsCaseInsensitive(textToMatch))
                        {
                            completions.Add(new CompletionItem(alias, CompletionItemKind.Keyword, detail: identifier.Description));
                        }
                    }
                }
            }
        }
    }
}
