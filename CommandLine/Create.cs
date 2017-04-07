// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Create
    {
        public static Option Option(
            string aliases,
            string help,
            ArgumentsRule arguments = null) =>
            new Option(
                aliases.Split(
                    new[] { '|', ' ' }, StringSplitOptions.RemoveEmptyEntries), help, arguments);

        public static Command Command(
            string name,
            string help) =>
            new Command(name, help);

        public static Command Command(
            string name,
            string help,
            params Option[] options) =>
            new Command(name, help, options);

        public static Command Command(
            string name,
            string help,
            bool treatUnmatchedTokensAsErrors,
            params Option[] options) =>
            new Command(name, help, options, treatUnmatchedTokensAsErrors: treatUnmatchedTokensAsErrors);

        public static Command Command(
            string name,
            string help,
            ArgumentsRule arguments,
            params Option[] options) =>
            new Command(name, help, options, arguments);

        public static Command Command(
            string name,
            string help,
            ArgumentsRule arguments,
            bool treatUnmatchedTokensAsErrors,
            params Option[] options) =>
            new Command(name, help, options, arguments, treatUnmatchedTokensAsErrors);

        public static Command Command(
            string name,
            string help,
            params Command[] commands) =>
            new Command(name, help, commands);
    }
}