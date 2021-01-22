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

        public Command Command { get; }

        public IEnumerable<Option> Options => Command.Options;

        internal void AddCommand(Command command) => Command.AddCommand(command);

        internal void AddOption(Option option) => Command.AddOption(option);

        internal void AddGlobalOption(Option option) => Command.AddGlobalOption(option);

        internal void AddArgument(Argument argument) => Command.AddArgument(argument);
    }
}
