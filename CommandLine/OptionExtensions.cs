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
            string.IsNullOrWhiteSpace(option.HelpText);

        internal static IEnumerable<AppliedOption> AllOptions(
            this AppliedOption option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            yield return option;

            foreach (var item in option.AppliedOptions.FlattenBreadthFirst(o => o.AppliedOptions))
            {
                yield return item;
            }
        }

        public static ParseResult Parse(
            this Option option,
            params string[] args) =>
            new Parser(option).Parse(args);

        public static ParseResult Parse(
            this Option option,
            string commandLine,
            char[] delimiters = null) =>
            new Parser(delimiters, option).Parse(commandLine);
    }
}