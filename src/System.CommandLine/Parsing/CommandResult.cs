// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
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

        public ICommand Command { get; }

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
