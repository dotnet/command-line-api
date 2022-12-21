// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="Command" />.
    /// </summary>
    public class CommandResult : SymbolResult
    {
        private Dictionary<Argument, ArgumentResult>? _defaultArgumentValues;

        internal CommandResult(
            Command command,
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
        public Command Command { get; }

        /// <summary>
        /// The token that was parsed to specify the command.
        /// </summary>
        public Token Token { get; }

        internal override bool UseDefaultValueFor(Argument argument) =>
            FindResultFor(argument) switch
            {
                ArgumentResult arg => arg.Argument.HasDefaultValue && 
                                      arg.Tokens.Count == 0,
                _ => false
            };

        internal ArgumentResult GetOrCreateDefaultArgumentResult(Argument argument) =>
            (_defaultArgumentValues ??= new()).GetOrAdd(
                argument,
                arg => new ArgumentResult(arg, this));
    }
}
