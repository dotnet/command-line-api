// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Collections;
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
        public SymbolSet Children { get; } = new();

        /// <summary>
        /// Represents all of the arguments for the command.
        /// </summary>
        public IReadOnlyList<Argument> Arguments => Children.Arguments;

        /// <summary>
        /// Represents all of the options for the command, including global options.
        /// </summary>
        public IReadOnlyList<Option> Options => Children.Options;

        /// <summary>
        /// Adds an <see cref="Argument"/> to the command.
        /// </summary>
        /// <param name="argument">The argument to add to the command.</param>
        public void AddArgument(Argument argument)
        {
            argument.AddParent(this);
            Children.AddWithoutAliasCollisionCheck(argument);
        }

        /// <summary>
        /// Adds a subcommand to the command.
        /// </summary>
        /// <param name="command">The subcommand to add to the command.</param>
        /// <remarks>Commands can be nested to an arbitrary depth.</remarks>
        public void AddCommand(Command command) => AddSymbol(command);

        /// <summary>
        /// Adds an <see cref="Option"/> to the command.
        /// </summary>
        /// <param name="option">The option to add to the command.</param>
        public void AddOption(Option option) => AddSymbol(option);

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
        /// Adds a <see cref="Symbol"/> to the command.
        /// </summary>
        /// <param name="symbol">The symbol to add to the command.</param>
        public void Add(Symbol symbol) => AddSymbol(symbol);

        /// <summary>
        /// Adds an <see cref="Argument"/> to the command.
        /// </summary>
        /// <param name="argument">The argument to add to the command.</param>
        public void Add(Argument argument) => AddArgument(argument);

        private protected override string DefaultName => throw new NotImplementedException();

        internal List<ValidateSymbolResult<CommandResult>> Validators { get; } = new();

        /// <summary>
        /// Adds a custom validator to the command. Validators can be used
        /// to create custom validation logic.
        /// </summary>
        /// <param name="validate">The delegate to validate the symbols during parsing.</param>
        public void AddValidator(ValidateSymbolResult<CommandResult> validate) => Validators.Add(validate);

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

        internal Parser? ImplicitParser { get; set; }

        private protected void AddSymbol(Symbol symbol)
        {
            Children.AddWithoutAliasCollisionCheck(symbol);
            symbol.AddParent(this);
        }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            var completions = new List<CompletionItem>();

            if (context.WordToComplete is { } textToMatch)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];

                    AddCompletionsFor(child);
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

            void AddCompletionsFor(Symbol child)
            {
                switch (child)
                {
                    case IdentifierSymbol identifier when !child.IsHidden:
                        foreach (var alias in identifier.Aliases)
                        {
                            if (alias is { } &&
                                alias.ContainsCaseInsensitive(textToMatch))
                            {
                                completions.Add(new CompletionItem(alias, CompletionItemKind.Keyword, detail: child.Description));
                            }
                        }

                        break;

                    case Argument argument:
                        foreach (var completion in argument.GetCompletions(context))
                        {
                            if (completion.Label.ContainsCaseInsensitive(textToMatch))
                            {
                                completions.Add(completion);
                            }
                        }

                        break;
                }
            }
        }
    }
}
