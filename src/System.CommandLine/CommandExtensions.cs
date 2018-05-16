// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace System.CommandLine
{
    public static class CommandExtensions
    {
        public static Command Subcommand(
            this Command command,
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            return command.DefinedSymbols.OfType<Command>().Single(c => c.Name == name);
        }
    }
}
