// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public static class CommandDefinitionExtensions
    {
        public static CommandDefinition Subcommand(
            this CommandDefinition commandDefinition,
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            return commandDefinition.SymbolDefinitions
                .OfType<CommandDefinition>()
                .Single(c => c.Name == name);
        }

        public static ParseResult Parse(
            this CommandDefinition commandDefinition,
            params string[] args) =>
            new Parser(new[] { commandDefinition }).Parse(args);

        public static ParseResult Parse(
            this CommandDefinition commandDefinition,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new Parser(new[] { commandDefinition }).Parse(commandLine);
    }
}
