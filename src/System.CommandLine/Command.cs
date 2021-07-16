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
        private readonly SymbolSet _globalOptions = new();

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
        /// Represents all of the global options for the command
        /// </summary>
        public IReadOnlyList<Option> GlobalOptions => _globalOptions.Options;

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
            _globalOptions.Add(option);
            Children.AddWithoutAliasCollisionCheck(option);
        }
        
        /// <summary>
        /// Adds a global <see cref="Option"/> to the command. A return value indicates whether the option alias is
        /// already in use.
        /// </summary>
        /// <param name="option">The global option to add to the command.</param>
        /// <returns><see langword="true"/> if the option was added;<see langword="false"/> if it was already in use.</returns>
        /// <remarks>Global options are applied to the command and recursively to subcommands. They do not apply to
        /// parent commands.</remarks>
        internal bool TryAddGlobalOption(Option option)
        {
            if (!_globalOptions.IsAnyAliasInUse(option, out _))
            {
                _globalOptions.Add(option);
                Children.AddWithoutAliasCollisionCheck(option);
                return true;
            }
            else
            {
                return false;
            }
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

        /// <summary>
        /// Adds an alias to the command. Multiple aliases can be added to a command, most often used to provide a
        /// shorthand alternative.
        /// </summary>
        /// <param name="alias">The alias to add to the command.</param>
        public virtual void AddAlias(string alias) => AddAliasInner(alias);

        private protected override void AddAliasInner(string alias)
        {
            ThrowIfAliasIsInvalid(alias);

            base.AddAliasInner(alias);
        }

        private protected override void AddSymbol(Symbol symbol)
        {
            if (symbol is IOption option)
            {
                _globalOptions.ThrowIfAnyAliasIsInUse(option);
            }
            
            base.AddSymbol(symbol);
        }

        private protected override string DefaultName => throw new NotImplementedException();

        internal List<ValidateSymbolResult<CommandResult>> Validators { get; } = new List<ValidateSymbolResult<CommandResult>>();

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
        /// <para>Use one of the <see cref="CommandHandler.Create(Action)" /> overloads to construct a handler.</para>
        /// <para>If the handler is not specified, parser errors will be generated for command line input that
        /// invokes this command.</para></remarks>
        public ICommandHandler? Handler { get; set; }

        /// <summary>
        /// Represents all of the symbols for the command.
        /// </summary>
        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IReadOnlyList<IArgument> ICommand.Arguments => Arguments;

        /// <inheritdoc />
        IReadOnlyList<IOption> ICommand.Options => Options;

        internal Parser? ImplicitParser { get; set; }
    }
}
