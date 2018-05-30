// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public static class OptionDefinitionExtensions
    {
        public static ParseResult Parse(
            this OptionDefinition optionDefinition,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new Parser(new CommandLineConfiguration(argumentDelimiters: delimiters, symbolDefinitions: new[] { optionDefinition })).Parse(commandLine);
    }
}
