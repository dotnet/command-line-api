// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine
{
    public static class ValidationMessages
    {
        private static IValidationMessages _current = new DefaultValidationMessages();
        private static IValidationMessages _default = _current;

        public static IValidationMessages Current
        {
            get => _current;
            set => _current = value ?? new DefaultValidationMessages();
        }

        public static string NoArgumentsAllowed(string option) =>
            _current.NoArgumentsAllowed(option).NotWhitespace() ??
            _default.NoArgumentsAllowed(option);

        public static string CommandAcceptsOnlyOneArgument(
            string command,
            int argumentCount) =>
            _current.CommandAcceptsOnlyOneArgument(command, argumentCount).NotWhitespace() ??
            _default.CommandAcceptsOnlyOneArgument(command, argumentCount);

        public static string FileDoesNotExist(string filePath) =>
            _current.FileDoesNotExist(filePath).NotWhitespace() ??
            _default.FileDoesNotExist(filePath);

        public static string OptionAcceptsOnlyOneArgument(
            string option,
            int argumentCount) =>
            _current.OptionAcceptsOnlyOneArgument(option, argumentCount).NotWhitespace() ??
            _default.OptionAcceptsOnlyOneArgument(option, argumentCount);

        public static string RequiredArgumentMissingForCommand(string command) =>
            _current.RequiredArgumentMissingForCommand(command).NotWhitespace() ??
            _default.RequiredArgumentMissingForCommand(command);

        public static string RequiredArgumentMissingForOption(string option) =>
            _current.RequiredArgumentMissingForOption(option).NotWhitespace() ??
            _default.RequiredArgumentMissingForOption(option);

        internal static string RequiredCommandWasNotProvided() =>
            _current.RequiredCommandWasNotProvided().NotWhitespace() ??
            _default.RequiredCommandWasNotProvided();

        internal static string SymbolAcceptsOnlyOneArgument(Symbol symbol) => symbol.SymbolDefinition is CommandDefinition
                   ? CommandAcceptsOnlyOneArgument(symbol.SymbolDefinition.ToString(), symbol.Arguments.Count)
                   : OptionAcceptsOnlyOneArgument(symbol.SymbolDefinition.ToString(), symbol.Arguments.Count);

        public static string UnrecognizedArgument(
            string unrecognizedArg,
            IReadOnlyCollection<string> allowedValues) =>
            _current.UnrecognizedArgument(unrecognizedArg, allowedValues).NotWhitespace() ??
            _default.UnrecognizedArgument(unrecognizedArg, allowedValues);

        internal static string UnrecognizedCommandOrArgument(string arg) =>
            _current.UnrecognizedCommandOrArgument(arg).NotWhitespace() ??
            _default.UnrecognizedCommandOrArgument(arg);

        public static string UnrecognizedOption(
            string unrecognizedOption,
            IReadOnlyCollection<string> allowedValues) =>
            _current.UnrecognizedOption(unrecognizedOption, allowedValues).NotWhitespace() ??
            _default.UnrecognizedOption(unrecognizedOption, allowedValues);

        internal static string ResponseFileNotFound(string filePath) =>
            _current.ResponseFileNotFound(filePath).NotWhitespace() ??
            _default.ResponseFileNotFound(filePath);

        internal static string ErrorReadingResponseFile(string filePath, IOException e) =>
            _current.ErrorReadingResponseFile(filePath, e).NotWhitespace() ??
            _default.ErrorReadingResponseFile(filePath, e);
    }
}
