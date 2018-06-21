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

        public virtual string ExpectsOneArgument(Symbol symbol) =>
            symbol is Command
                ? $"Command '{symbol.Token}' expects a single argument but {symbol.Arguments.Count} were provided."
                : $"Option '{symbol.Token}' expects a single argument but {symbol.Arguments.Count} were provided.";

        public virtual string ExpectsFewerArguments(Symbol symbol, int maximumNumberOfArguments) =>
            symbol is Command
                ? $"Command '{symbol.Token}' expects no more than {maximumNumberOfArguments} arguments, but {symbol.Arguments.Count} were provided."
                : $"Option '{symbol.Token}' expects no more than {maximumNumberOfArguments} arguments, but {symbol.Arguments.Count} were provided.";

        public virtual string FileDoesNotExist(string filePath) =>
            $"File does not exist: {filePath}";

        public virtual string NoArgumentsAllowed(Symbol symbol) =>
            $"Arguments not allowed for option: {symbol.Token}";

        public virtual string RequiredArgumentMissing(Symbol symbol) =>
            symbol is Command
                ? $"Required argument missing for command: {symbol.Token}"
                : $"Required argument missing for option: {symbol.Token}";

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
