// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="ICommand" />.
    /// </summary>
    public class CommandResult : SymbolResult
    {
        internal CommandResult(
            ICommand command,
            Token token,
            CommandResult? parent = null) :
            base(command ?? throw new ArgumentNullException(nameof(command)),
                 parent)
        {
            Command = command;

            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        /// <summary>
        /// The command to which the result applies.
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// The token that was parsed to specify the command.
        /// </summary>
        public Token Token { get; }

        internal override bool UseDefaultValueFor(IArgument argument) =>
            Children.ResultFor(argument) switch
            {
                ArgumentResult arg => arg.Argument.HasDefaultValue && 
                                      arg.Tokens.Count == 0,
                _ => false
            };
    }
}
