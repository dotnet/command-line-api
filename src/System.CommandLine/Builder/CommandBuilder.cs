// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
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

        public IEnumerable<Option> Options => Command.Children.OfType<Option>();

        internal ICommandHandler Handler
        {
            get => Command.Handler;
            set => Command.Handler = value;
        }

        internal void AddCommand(Command command) => Command.AddCommand(command);

        internal void AddOption(Option option) => Command.AddOption(option);
    }
}
