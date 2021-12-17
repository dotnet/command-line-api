// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Collections;
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
    public class Command : 
        IdentifierSymbol, 
        ICommand, 
        IEnumerable<Symbol>
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
        public void AddArgument(Argument argument) => AddArgumentInner(argument);

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
        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        /// <summary>
        /// Validates all symbols including the child hierarchy.
        /// </summary>
        /// <remarks>Due to the performance impact of this method, it's recommended to create
        /// a Unit Test that calls this method to verify the RootCommand of every application.</remarks>
        public void Validate()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                for (int j = 1; j < Children.Count; j++)
                {
                    if (Children[j] is IdentifierSymbol identifierSymbol)
                    {
                        foreach (string alias in identifierSymbol.Aliases)
                        {
                            if (Children[i].Matches(alias))
                            {
                                throw new ArgumentException($"Alias '{alias}' is already in use.");
                            }
                        }

                        if (identifierSymbol is Command command)
                        {
                            if (ReferenceEquals(this, command))
                            {
                                throw new ArgumentException("Parent can't be it's own child.");
                            }

                            command.Validate();
                        }
                    }

                    if (Children[i].Matches(Children[j].Name))
                    {
                        throw new ArgumentException($"Alias '{Children[j].Name}' is already in use.");
                    }
                }

                if (Children.Count == 1 && Children[0] is Command singleCommand)
                {
                    if (ReferenceEquals(this, singleCommand))
                    {
                        throw new ArgumentException("Parent can't be it's own child.");
                    }

                    singleCommand.Validate();
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IReadOnlyList<IArgument> ICommand.Arguments => Arguments;

        /// <inheritdoc />
        IReadOnlyList<IOption> ICommand.Options => Options;

        internal Parser? ImplicitParser { get; set; }
    }
}
