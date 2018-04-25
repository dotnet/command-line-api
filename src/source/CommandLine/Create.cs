// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Create
    {
        public static Option Option(
            string aliases,
            string description,
            ArgumentsRule arguments = null) =>
            new Option(
                aliases.Split(
                    new[] { '|', ' ' }, StringSplitOptions.RemoveEmptyEntries),
                description,
                arguments: arguments);

        public static Command Command(
            string name,
            string description) =>
            new Command(name, description, ArgumentsRule.None);

        public static Command Command(
            string name,
            string description,
            params Symbol[] options) =>
            new Command(name, description, options);

        public static Command Command(
            string name,
            string description,
            bool treatUnmatchedTokensAsErrors,
            params Symbol[] symbols) =>
            new Command(name, description, symbols, treatUnmatchedTokensAsErrors: treatUnmatchedTokensAsErrors);

        public static Command Command(
            string name,
            string description,
            ArgumentsRule arguments,
            params Symbol[] symbols) =>
            new Command(name, description, symbols, arguments);

        public static Command Command(
            string name,
            string description,
            ArgumentsRule arguments,
            bool treatUnmatchedTokensAsErrors,
            params Symbol[] options) =>
            new Command(name, description, options, arguments, treatUnmatchedTokensAsErrors);

        public static Command Command(
            string name,
            string description,
            params Command[] commands) =>
            new Command(name, description, commands);

        public static Command RootCommand(params Symbol[] symbols) =>
            new Command(symbols);
    }
}