// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentExtensions
    {
        public static ParseResult Parse(
            this Argument argument,
            string commandLine) =>
            new Parser(
                new CommandLineConfiguration(
                    new[]
                    {
                        new RootCommand
                        {
                            argument
                        },
                    })).Parse(commandLine);
    }
}