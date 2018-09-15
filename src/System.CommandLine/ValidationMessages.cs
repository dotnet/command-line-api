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

        protected ValidationMessages()
        {
        }

        public virtual string ExpectsOneArgument(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"Command '{symbolResult.Token}' expects a single argument but {symbolResult.Arguments.Count} were provided."
                : $"Option '{symbolResult.Token}' expects a single argument but {symbolResult.Arguments.Count} were provided.";

        public virtual string ExpectsFewerArguments(SymbolResult symbolResult, int maximumNumberOfArguments) =>
            symbolResult is CommandResult
                ? $"Command '{symbolResult.Token}' expects no more than {maximumNumberOfArguments} arguments, but {symbolResult.Arguments.Count} were provided."
                : $"Option '{symbolResult.Token}' expects no more than {maximumNumberOfArguments} arguments, but {symbolResult.Arguments.Count} were provided.";

        public virtual string FileDoesNotExist(string filePath) =>
            $"File does not exist: {filePath}";

        public virtual string NoArgumentsAllowed(SymbolResult symbolResult) =>
            $"Arguments not allowed for option: {symbolResult.Token}";

        public virtual string RequiredArgumentMissing(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"Required argument missing for command: {symbolResult.Token}"
                : $"Required argument missing for option: {symbolResult.Token}";

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
