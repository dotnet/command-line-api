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
        NamedSymbol, 
        ICommand, 
        IEnumerable<Symbol>
    {
        private readonly SymbolSet _globalOptions = new SymbolSet();

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command, shown in help.</param>
        public Command(string name, string? description = null) : base(name, description)
        {
        }

        /// <summary>
        /// Represents all of the arguments associated with the command.
        /// </summary>
        public IEnumerable<Argument> Arguments => 
            Children.OfType<Argument>();

        /// <summary>
        /// Represents all of the options associated with the command, including global options.
        /// </summary>
        public IEnumerable<Option> Options =>
            Children.OfType<Option>()
                    .Concat(Parents
                            .OfType<Command>()
                            .SelectMany(c => c.GlobalOptions));
        /// <summary>
        /// Represents all of the global options associated with the command
        /// </summary>
        public IEnumerable<Option> GlobalOptions => _globalOptions.OfType<Option>();

        /// <summary>
        /// Adds an <see cref="Argument"/> to the command.
        /// </summary>
        /// <param name="argument">The argument to add to the command.</param>
        public void AddArgument(Argument argument) => AddArgumentInner(argument);

        /// <summary>
        /// Adds a subcommand to the command. Commands can be nested to an arbitrary depth.
        /// </summary>
        /// <param name="command">The subcommand to add to the command.</param>
        public void AddCommand(Command command) => AddSymbol(command);

        /// <summary>
        /// Adds an <see cref="Option"/> to the command.
        /// </summary>
        /// <param name="option">The option to add to the command.</param>
        public void AddOption(Option option) => AddSymbol(option);

        /// <summary>
        /// Adds a global <see cref="Option"/> to the command. Global options are applied to all commands.
        /// </summary>
        /// <param name="option">The global option to add to the command.</param>
        public void AddGlobalOption(Option option)
        {
            _globalOptions.Add(option);
            Children.AddWithoutAliasCollisionCheck(option);
        }
        
        /// <summary>
        /// Adds a global <see cref="Option"/> to the command. Global options are applied to all commands. A
        /// return value indicates whether the option alias is already in use.
        /// </summary>
        /// <param name="option">The global option to add to the command.</param>
        /// <returns><c>true</c> if the option was added;<c>false</c> if it was already in use.</returns>
        public bool TryAddGlobalOption(Option option)
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
        /// <param name="alias">A string representing the alias to add to the command.</param>
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

            symbol.AddParent(this);

            base.AddSymbol(symbol);
        }

        private protected override string DefaultName => throw new NotImplementedException();

        internal List<ValidateSymbol<CommandResult>> Validators { get; } = new List<ValidateSymbol<CommandResult>>();

        /// <summary>
        /// Adds a custom <see cref="ValidateSymbol{T}(CommandResult)"/> to the command. Validators can be used
        /// to create custom validation logic.
        /// </summary>
        /// <param name="validate">The delegate to validate the symbols during parsing.</param>
        public void AddValidator(ValidateSymbol<CommandResult> validate) => Validators.Add(validate);

        /// <summary>
        /// Gets or sets a value that indicates whether unmatched tokens should be treated as errors. For example,
        /// if set to <c>true</c> and an extra command or argument is provided, validation will fail.
        /// </summary>
        public bool TreatUnmatchedTokensAsErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="ICommandHandler"/> for the command. The handler represents the action
        /// that will be performed when the command is invoked.
        /// </summary>
        public ICommandHandler? Handler { get; set; }

        /// <summary>
        /// Represents all of the symbols associated with the command.
        /// </summary>
        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerable<IArgument> ICommand.Arguments => Arguments;

        IEnumerable<IOption> ICommand.Options => Options;

        internal Parser? ImplicitParser { get; set; }
    }
}
