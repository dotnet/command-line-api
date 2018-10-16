// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class OptionExtensions
    {
        public static ParseResult Parse(
            this Option option,
            string commandLine) =>
            new Parser(new CommandLineConfiguration(new[] { option })).Parse(commandLine);
    }
}
