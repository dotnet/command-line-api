﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="Command" />.
    /// </summary>
    public class CommandResult : SymbolResult
    {
        internal CommandResult(
            Command command,
            Token token,
            CommandResult? parent = null) :
            base(parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
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

        /// <summary>
        /// Child symbol results in the parse tree.
        /// </summary>
        public IEnumerable<SymbolResult> Children => GetChildren(this);

        internal sealed override int MaximumArgumentCapacity
        {
            get
            {
                var value = 0;

                if (Command.HasArguments)
                {
                    var arguments = Command.Arguments;

                    for (var i = 0; i < arguments.Count; i++)
                    {
                        value += arguments[i].Arity.MaximumNumberOfValues;
                    }
                }

                return value;
            }
        }

        internal override bool UseDefaultValueFor(Argument argument) =>
            FindResultFor(argument) switch
            {
                ArgumentResult arg => arg.Argument.HasDefaultValue && 
                                      arg.Tokens.Count == 0,
                _ => false
            };
    }
}
