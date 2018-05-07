// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public static class ValidationMessages
    {
        private static IValidationMessages current = new DefaultValidationMessages();
        private static IValidationMessages @default = current;

        public static IValidationMessages Current
        {
            get => current;
            set => current = value ?? new DefaultValidationMessages();
        }

        public static string NoArgumentsAllowed(string option) =>
            current.NoArgumentsAllowed(option).NotWhitespace() ??
            @default.NoArgumentsAllowed(option);

        public static string CommandAcceptsOnlyOneArgument(
            string command,
            int argumentCount) =>
            current.CommandAcceptsOnlyOneArgument(command, argumentCount).NotWhitespace() ??
            @default.CommandAcceptsOnlyOneArgument(command, argumentCount);

        public static string CommandAcceptsOnlyOneSubcommand(
            string command,
            string subcommandsSpecified) =>
            current.CommandAcceptsOnlyOneSubcommand(command, subcommandsSpecified).NotWhitespace() ??
            @default.CommandAcceptsOnlyOneSubcommand(command, subcommandsSpecified);

        public static string FileDoesNotExist(string filePath) =>
            current.FileDoesNotExist(filePath).NotWhitespace() ??
            @default.FileDoesNotExist(filePath);

        public static string OptionAcceptsOnlyOneArgument(
            string option,
            int argumentCount) =>
            current.OptionAcceptsOnlyOneArgument(option, argumentCount).NotWhitespace() ??
            @default.OptionAcceptsOnlyOneArgument(option, argumentCount);

        public static string RequiredArgumentMissingForCommand(string command) =>
            current.RequiredArgumentMissingForCommand(command).NotWhitespace() ??
            @default.RequiredArgumentMissingForCommand(command);

        public static string RequiredArgumentMissingForOption(string option) =>
            current.RequiredArgumentMissingForOption(option).NotWhitespace() ??
            @default.RequiredArgumentMissingForOption(option);

        internal static string RequiredCommandWasNotProvided() =>
            current.RequiredCommandWasNotProvided().NotWhitespace() ??
            @default.RequiredCommandWasNotProvided();

        internal static string SymbolAcceptsOnlyOneArgument(ParsedSymbol parsedSymbol) => parsedSymbol.Symbol is Command
                   ? CommandAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), parsedSymbol.Arguments.Count)
                   : OptionAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), parsedSymbol.Arguments.Count);

        public static string UnrecognizedArgument(
            string unrecognizedArg,
            IReadOnlyCollection<string> allowedValues) =>
            current.UnrecognizedArgument(unrecognizedArg, allowedValues).NotWhitespace() ??
            @default.UnrecognizedArgument(unrecognizedArg, allowedValues);

        internal static string UnrecognizedCommandOrArgument(string arg) =>
            current.UnrecognizedCommandOrArgument(arg).NotWhitespace() ??
            @default.UnrecognizedCommandOrArgument(arg);

        public static string UnrecognizedOption(
            string unrecognizedOption,
            IReadOnlyCollection<string> allowedValues) =>
            current.UnrecognizedOption(unrecognizedOption, allowedValues).NotWhitespace() ??
            @default.UnrecognizedOption(unrecognizedOption, allowedValues);
    }
}
