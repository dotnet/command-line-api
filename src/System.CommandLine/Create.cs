// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace System.CommandLine
{
    public static class Create
    {
        public static CommandDefinition Command(
            string name,
            string description,
            ArgumentDefinition arguments = null,
            params SymbolDefinition[] symbolDefinitions) =>
            new CommandDefinition(name, description, symbolDefinitions, arguments);

        public static CommandDefinition Command(
            string name,
            string description,
            params CommandDefinition[] commandDefinitions) =>
            new CommandDefinition(name, description, commandDefinitions);
    }
}
