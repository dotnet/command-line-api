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
                ? $"Command '{symbolResult.Token.Value}' expects a single argument but {symbolResult.Tokens.Count} were provided."
                : $"Option '{symbolResult.Token.Value}' expects a single argument but {symbolResult.Tokens.Count} were provided.";

        public virtual string NoArgumentProvided(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"No argument was provided for Command '{symbolResult.Token.Value}'."
                : $"No argument was provided for Option '{symbolResult.Token.Value}'.";

        public virtual string ExpectsFewerArguments(
            Token token, 
            int providedNumberOfValues,
            int maximumNumberOfValues) =>
            token.Type == TokenType.Command
                ? $"Command '{token}' expects no more than {maximumNumberOfValues} arguments, but {providedNumberOfValues} were provided."
                : $"Option '{token}' expects no more than {maximumNumberOfValues} arguments, but {providedNumberOfValues} were provided.";

        public virtual string DirectoryDoesNotExist(string path) =>
            $"Directory does not exist: {path}";

        public virtual string FileDoesNotExist(string filePath) =>
            $"File does not exist: {filePath}";

        public virtual string InvalidCharactersInPath(char invalidChar) =>
            $"Character not allowed in a path: {invalidChar}";

        public virtual string RequiredArgumentMissing(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"Required argument missing for command: {symbolResult.Token.Value}"
                : $"Required argument missing for option: {symbolResult.Token.Value}";

        public virtual string RequiredCommandWasNotProvided() =>
            "Required command was not provided.";

        public virtual string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) =>
            $"Argument '{unrecognizedArg}' not recognized. Must be one of:\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}";

        public virtual string UnrecognizedCommandOrArgument(string arg) =>
            $"Unrecognized command or argument '{arg}'";

        public virtual string ResponseFileNotFound(string filePath) =>
            $"Response file not found '{filePath}'";

        public virtual string ErrorReadingResponseFile(string filePath, IOException e) =>
            $"Error reading response file '{filePath}': {e.Message}";
    }
}
