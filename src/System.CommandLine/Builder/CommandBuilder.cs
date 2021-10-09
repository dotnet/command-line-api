// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    /// <summary>
    /// Enables composition of command line configurations.
    /// </summary>
    public class CommandBuilder
    {
        /// <param name="command"></param>
        internal CommandBuilder(Command command) 
        {
            Command = command;
        }

        /// <summary>
        /// The command that the builder uses as its configuration root.
        /// </summary>
        public Command Command { get; }
    }
}