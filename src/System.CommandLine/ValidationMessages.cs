// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public class ValidationMessages
    {
        public static ValidationMessages Instance { get; } = new ValidationMessages();

        protected ValidationMessages() { }

        public virtual string CommandExpectsOneArgument(CommandDefinition command, int argumentCount) =>
            $"Command '{command.Name}' expects a single argument but {argumentCount} were provided.";

        public virtual string FileDoesNotExist(string filePath) =>
            $"File does not exist: {filePath}";

        public virtual string NoArgumentsAllowed(SymbolDefinition symbol) =>
            $"Arguments not allowed for option: {symbol.Token()}";

        public virtual string OptionExpectsOneArgument(OptionDefinition option, int argumentCount) =>
            $"Option '{option.Token()}' expects a single argument but {argumentCount} were provided.";

        public virtual string RequiredArgumentMissingForCommand(CommandDefinition command) =>
            $"Required argument missing for command: {command.Name}";

        public virtual string RequiredArgumentMissingForOption(OptionDefinition option) =>
            $"Required argument missing for option: {option.Token()}";

        public virtual string RequiredCommandWasNotProvided() =>
            "Required command was not provided.";

        public virtual string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) =>
            $"Argument '{unrecognizedArg}' not recognized. Must be one of:\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}";

        public virtual string UnrecognizedCommandOrArgument(string arg) =>
            $"Unrecognized command or argument '{arg}'";

        public virtual string UnrecognizedOption(string unrecognizedOption, IReadOnlyCollection<string> allowedValues) =>
            $"Option '{unrecognizedOption}' not recognized. Must be one of:\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}";

        public virtual string ResponseFileNotFound(string filePath) =>
            $"Response file not found '{filePath}'";

        public virtual string ErrorReadingResponseFile(string filePath, IOException e) =>
            $"Error reading response file '{filePath}': {e.Message}";
    }
}
