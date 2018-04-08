// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class OptionExtensions
    {
        internal static IEnumerable<Option> FlattenBreadthFirst(
            this IEnumerable<Option> options)
        {
            foreach (var item in options.FlattenBreadthFirst(o => o.DefinedOptions))
            {
                yield return item;
            }
        }

        public static Command Command(this Option option) =>
            option.RecurseWhileNotNull(o => o.Parent)
                  .OfType<Command>()
                  .FirstOrDefault();

        public static bool IsHidden(this Option option) =>
            string.IsNullOrWhiteSpace(option.Description);

        internal static IEnumerable<ParsedOption> AllOptions(
            this ParsedOption option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            yield return option;

            foreach (var item in option.ParsedOptions.FlattenBreadthFirst(o => o.ParsedOptions))
            {
                yield return item;
            }
        }

        public static ParseResult Parse(
            this Option option,
            params string[] args) =>
            new OptionParser(option).Parse(args);

        public static ParseResult Parse(
            this Option option,
            string commandLine,
            char[] delimiters = null) =>
            new OptionParser(new ParserConfiguration(argumentDelimiters: delimiters, definedOptions:new[] {option})).Parse(commandLine);

        public static ParseResult Parse(
            this Command command,
            params string[] args) =>
            new CommandParser(command).Parse(args);

        public static ParseResult Parse(
            this Command command,
            string commandLine,
            char[] delimiters = null) =>
            new CommandParser( command).Parse(commandLine);
    }
}