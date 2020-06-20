// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
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

        public virtual string ExpectsOneArgument(SymbolResult symbolResult)
        {
            return symbolResult is CommandResult
                       ? $"Command '{symbolResult.Token().Value}' expects a single argument but {symbolResult.Tokens.Count} were provided."
                       : $"Option '{symbolResult.Token().Value}' expects a single argument but {symbolResult.Tokens.Count} were provided.";
        }

        public virtual string NoArgumentProvided(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"No argument was provided for Command '{symbolResult.Token().Value}'."
                : $"No argument was provided for Option '{symbolResult.Token().Value}'.";

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

        public virtual string FileOrDirectoryDoesNotExist(string path) =>
            $"File or directory does not exist: {path}";

        public virtual string InvalidCharactersInPath(char invalidChar) =>
            $"Character not allowed in a path: {invalidChar}";

        public virtual string RequiredArgumentMissing(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? $"Required argument missing for command: {symbolResult.Token().Value}"
                : $"Required argument missing for option: {symbolResult.Token().Value}";

        public virtual string RequiredCommandWasNotProvided() =>
            "Required command was not provided.";

        public virtual string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) =>
            $"Argument '{unrecognizedArg}' not recognized. Must be one of:\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}";

        public virtual string UnrecognizedCommandOrArgument(string arg, ISymbol? symbol)
        {
            var possible = symbol switch
            {
                null => "",
                Command command => ExpectedCommandsAndArguments(command),
                Option option => ExpectedArguments(option),
                _ => throw new InvalidOperationException("Unexpected symbol type")
            };

            return $"Unrecognized command or argument '{arg}'. {possible}.";
        }

        public virtual string ResponseFileNotFound(string filePath) =>
            $"Response file not found '{filePath}'";

        public virtual string ErrorReadingResponseFile(string filePath, IOException e) =>
            $"Error reading response file '{filePath}': {e.Message}";

        private static string ExpectedArguments(Option option)
            => (option.Argument is Argument argument)
                ? $"Available {ExpectedArgument(argument)}"
                : "Available no arguments";

        private static string ExpectedArgument(Argument argument)
        {
            return $"<{ argument.Name}> as {ArgumentType(argument.ArgumentType)}{ArityIfNeeded(argument.Arity)}";

            static string ArityIfNeeded(IArgumentArity arity)
            {
                return (arity.MaximumNumberOfValues != int.MaxValue && arity.MinimumNumberOfValues != 0)
                    ? arity.MinimumNumberOfValues == arity.MaximumNumberOfValues
                      ? $" ({arity.MinimumNumberOfValues} values)"
                      : $" ({arity.MinimumNumberOfValues} to {arity.MaximumNumberOfValues} values)"
                    : arity.MaximumNumberOfValues != int.MaxValue
                      ? $" (not more than {arity.MaximumNumberOfValues} values)"
                      : arity.MinimumNumberOfValues != 0
                        ? $" (at least {arity.MaximumNumberOfValues} values)"
                        : "";
            }

            static string ArgumentType(Type type)
            {
                return type.ToString() == "System.Void"
                        ? "<unknown type>"
                        : type.ToString();
            }
        }

        private static string ExpectedCommandsAndArguments(Command command)
        {
            var ret = "";
            var subCommands = command.Children.OfType<Command>().ToList();
            var arguments = command.Arguments;
            if (!(subCommands.Any() || arguments.Any()))
            {
                return "No commands or arguments are available";
            }
            if (subCommands.Any())
            {
                var subCommandNames = subCommands.Select(x => x.Name).Distinct();
                ret += $"Available command {string.Join(", or ", subCommandNames)}";
            }
            if (arguments.Any())
            {
                var argumentsDisplay = arguments.Select(x => ExpectedArgument(x));
                var display = string.Join(", or ", argumentsDisplay);
                ret += string.IsNullOrEmpty(ret)
                         ? $"Available argument {display}"
                         : $", or argument {display}";
            }

            return ret;
        }
    }
}
