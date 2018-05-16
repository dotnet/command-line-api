// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace System.CommandLine
{
    public static class Create
    {
        public static OptionDefinition Option(
            string aliases,
            string description,
            ArgumentDefinition argumentDefinition = null) =>
            new OptionDefinition(
                aliases.Split(
                    new[] { '|', ' ' }, StringSplitOptions.RemoveEmptyEntries),
                description,
                argumentDefinition: argumentDefinition);

        public static CommandDefinition Command(
            string name,
            string description) =>
            new CommandDefinition(name, description, ArgumentDefinition.None);

        public static CommandDefinition Command(
            string name,
            string description,
            params SymbolDefinition[] options) =>
            new CommandDefinition(name, description, options);

        public static CommandDefinition Command(
            string name,
            string description,
            bool treatUnmatchedTokensAsErrors,
            params SymbolDefinition[] symbolsDefinition) =>
            new CommandDefinition(name, description, symbolsDefinition, treatUnmatchedTokensAsErrors: treatUnmatchedTokensAsErrors);

        public static CommandDefinition Command(
            string name,
            string description,
            ArgumentDefinition arguments = null,
            params SymbolDefinition[] symbolsDefinition) =>
            new CommandDefinition(name, description, symbolsDefinition, arguments);

        public static CommandDefinition Command(
            string name,
            string description,
            ArgumentDefinition arguments,
            bool treatUnmatchedTokensAsErrors,
            params SymbolDefinition[] options) =>
            new CommandDefinition(name, description, options, arguments, treatUnmatchedTokensAsErrors);

        public static CommandDefinition Command(
            string name,
            string description,
            params CommandDefinition[] commandDefinitions) =>
            new CommandDefinition(name, description, commandDefinitions);

        public static CommandDefinition RootCommand(params SymbolDefinition[] symbolsDefinition) =>
            new CommandDefinition(symbolsDefinition);
    }
}
