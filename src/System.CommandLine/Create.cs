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
            params SymbolDefinition[] symbolDefinitions) =>
            new CommandDefinition(name, description, symbolDefinitions);

        public static CommandDefinition Command(
            string name,
            string description,
            bool treatUnmatchedTokensAsErrors,
            params SymbolDefinition[] symbolDefinitions) =>
            new CommandDefinition(name, description, symbolDefinitions, treatUnmatchedTokensAsErrors: treatUnmatchedTokensAsErrors);

        public static CommandDefinition Command(
            string name,
            string description,
            ArgumentDefinition arguments = null,
            params SymbolDefinition[] symbolDefinitions) =>
            new CommandDefinition(name, description, symbolDefinitions, arguments);

        public static CommandDefinition Command(
            string name,
            string description,
            ArgumentDefinition arguments,
            bool treatUnmatchedTokensAsErrors,
            params SymbolDefinition[] symbolDefinitions) =>
            new CommandDefinition(name, description, symbolDefinitions, arguments, treatUnmatchedTokensAsErrors);

        public static CommandDefinition Command(
            string name,
            string description,
            params CommandDefinition[] commandDefinitions) =>
            new CommandDefinition(name, description, commandDefinitions);

        public static CommandDefinition RootCommand(params SymbolDefinition[] symbolsDefinition) =>
            new CommandDefinition(symbolsDefinition);
    }
}
