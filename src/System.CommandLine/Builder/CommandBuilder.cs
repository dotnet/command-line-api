// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class CommandBuilder
    {
        public CommandBuilder(Command command)
        {
            Command = command;
        }

        private protected CommandBuilder()
        { }

        public Command? Command { get; private set; }

        public IEnumerable<Option> Options => Command.Children.OfType<Option>();

        private protected void SetCommandInternal(Command command)
        {
            if (!(Command is null))
            {
                throw new InvalidOperationException("Command has already been set.");
            }
            Command = command;
        }

        internal void AddCommand(Command command) => Command.AddCommand(command);

        internal void AddOption(Option option) => Command.AddOption(option);

        internal void AddGlobalOption(Option option) => Command.AddGlobalOption(option);

        internal void AddArgument(Argument argument) => Command.AddArgument(argument);
    }
}
