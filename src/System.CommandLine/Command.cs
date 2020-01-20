// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Represents a command or subcommand.
    /// </summary>
    public class Command : Symbol, ICommand, IEnumerable<Symbol>
    {
        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command. This will appear in help.</param>
        public Command(string name, string description = null) : base(new[] { name }, description)
        {
        }

        /// <summary>
        /// A list of the Arguments that have been defined for the command.
        /// </summary>
        public IEnumerable<Argument> Arguments => Children.OfType<Argument>();

        /// <summary>
        /// A list of the Options that have been defined for the command
        /// </summary>
        public IEnumerable<Option> Options => Children.OfType<Option>();

        /// <summary>
        /// Add a new argument to the command.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(Argument argument) => AddArgumentInner(argument);

        /// <summary>
        /// Add a new subcommand to the command.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(Command command) => AddSymbol(command);

        /// <summary>
        /// Add a new option to the command.
        /// </summary>
        /// <param name="option">The option to add.</param>
        public void AddOption(Option option) => AddSymbol(option);

        /// <summary>
        /// Add a new command, option or argument to the command.
        /// </summary>
        /// <param name="symbol">The command, option or argument to add.</param>
        public void Add(Symbol symbol) => AddSymbol(symbol);

        public void Add(Argument argument) => AddArgument(argument);

        internal List<ValidateSymbol<CommandResult>> Validators { get; } = new List<ValidateSymbol<CommandResult>>();

        /// <summary>
        /// Add a delegate that does custom validation for a command. 
        /// </summary>
        /// <param name="validate">The delegate that contains the custom validation.</param>
        public void AddValidator(ValidateSymbol<CommandResult> validate) => Validators.Add(validate);

        /// <summary>
        /// If this value is true, extra tokens will be reported as an error.
        /// </summary>
        public bool TreatUnmatchedTokensAsErrors { get; set; } = true;

        /// <summary>
        /// The action to perform when the command is invoked. 
        /// </summary>
        public ICommandHandler Handler { get; set; }

        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerable<IArgument> ICommand.Arguments => Arguments;

        IEnumerable<IOption> ICommand.Options => Options;
    }
}
