// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Provides localizable strings for help and error messages.
    /// </summary>
    internal static class LocalizationResources
    {
        /// <summary>
        ///   Interpolates values into a localized string similar to Command &apos;{0}&apos; expects a single argument but {1} were provided.
        /// </summary>
        internal static string ExpectsOneArgument(OptionResult optionResult)
            => GetResourceString(Properties.Resources.OptionExpectsOneArgument, GetOptionName(optionResult), optionResult.Tokens.Count);

        /// <summary>
        ///   Interpolates values into a localized string similar to Directory does not exist: {0}.
        /// </summary>
        internal static string DirectoryDoesNotExist(string path) =>
            GetResourceString(Properties.Resources.DirectoryDoesNotExist, path);

        /// <summary>
        ///   Interpolates values into a localized string similar to File does not exist: {0}.
        /// </summary>
        internal static string FileDoesNotExist(string filePath) =>
            GetResourceString(Properties.Resources.FileDoesNotExist, filePath);

        /// <summary>
        ///   Interpolates values into a localized string similar to File or directory does not exist: {0}.
        /// </summary>
        internal static string FileOrDirectoryDoesNotExist(string path) =>
            GetResourceString(Properties.Resources.FileOrDirectoryDoesNotExist, path);

        /// <summary>
        ///   Interpolates values into a localized string similar to Character not allowed in a path: {0}.
        /// </summary>
        internal static string InvalidCharactersInPath(char invalidChar) =>
            GetResourceString(Properties.Resources.InvalidCharactersInPath, invalidChar);

        /// <summary>
        ///   Interpolates values into a localized string similar to Character not allowed in a file name: {0}.
        /// </summary>
        internal static string InvalidCharactersInFileName(char invalidChar) =>
            GetResourceString(Properties.Resources.InvalidCharactersInFileName, invalidChar);

        /// <summary>
        ///   Interpolates values into a localized string similar to Required argument missing for command: {0}.
        /// </summary>
        internal static string RequiredArgumentMissing(ArgumentResult argumentResult) =>
            argumentResult.Parent is CommandResult commandResult
                ? GetResourceString(Properties.Resources.CommandRequiredArgumentMissing, commandResult.IdentifierToken.Value)
                : RequiredArgumentMissing((OptionResult)argumentResult.Parent!);

        /// <summary>
        ///   Interpolates values into a localized string similar to Required argument missing for option: {0}.
        /// </summary>
        internal static string RequiredArgumentMissing(OptionResult optionResult) =>
            GetResourceString(Properties.Resources.OptionRequiredArgumentMissing, GetOptionName(optionResult));

        /// <summary>
        ///   Interpolates values into a localized string similar to Required command was not provided.
        /// </summary>
        internal static string RequiredCommandWasNotProvided() =>
            GetResourceString(Properties.Resources.RequiredCommandWasNotProvided);

        /// <summary>
        ///   Interpolates values into a localized string similar to Option '{0}' is required.
        /// </summary>
        internal static string RequiredOptionWasNotProvided(string longestAliasWithPrefix) =>
            GetResourceString(Properties.Resources.RequiredOptionWasNotProvided, longestAliasWithPrefix);

        /// <summary>
        ///   Interpolates values into a localized string similar to Argument &apos;{0}&apos; not recognized. Must be one of:{1}.
        /// </summary>
        internal static string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) =>
            GetResourceString(Properties.Resources.UnrecognizedArgument, unrecognizedArg, $"\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}");

        /// <summary>
        ///   Interpolates values into a localized string similar to Unrecognized command or argument &apos;{0}&apos;.
        /// </summary>
        internal static string UnrecognizedCommandOrArgument(string arg) =>
            GetResourceString(Properties.Resources.UnrecognizedCommandOrArgument, arg);

        /// <summary>
        ///   Interpolates values into a localized string similar to Response file not found &apos;{0}&apos;.
        /// </summary>
        internal static string ResponseFileNotFound(string filePath) =>
            GetResourceString(Properties.Resources.ResponseFileNotFound, filePath);

        /// <summary>
        ///   Interpolates values into a localized string similar to Error reading response file &apos;{0}&apos;: {1}.
        /// </summary>
        internal static string ErrorReadingResponseFile(string filePath, IOException e) =>
            GetResourceString(Properties.Resources.ErrorReadingResponseFile, filePath, e.Message);

        /// <summary>
        ///   Interpolates values into a localized string similar to Show help and usage information.
        /// </summary>
        internal static string HelpOptionDescription() =>
            GetResourceString(Properties.Resources.HelpOptionDescription);

        /// <summary>
        ///   Interpolates values into a localized string similar to Usage:.
        /// </summary>
        internal static string HelpUsageTitle() =>
            GetResourceString(Properties.Resources.HelpUsageTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to Description:.
        /// </summary>
        internal static string HelpDescriptionTitle() =>
            GetResourceString(Properties.Resources.HelpDescriptionTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to [options].
        /// </summary>
        internal static string HelpUsageOptions() =>
            GetResourceString(Properties.Resources.HelpUsageOptions);

        /// <summary>
        ///   Interpolates values into a localized string similar to [command].
        /// </summary>
        internal static string HelpUsageCommand() =>
            GetResourceString(Properties.Resources.HelpUsageCommand);

        /// <summary>
        ///   Interpolates values into a localized string similar to [[--] &lt;additional arguments&gt;...]].
        /// </summary>
        internal static string HelpUsageAdditionalArguments() =>
            GetResourceString(Properties.Resources.HelpUsageAdditionalArguments);

        /// <summary>
        ///   Interpolates values into a localized string similar to Arguments:.
        /// </summary>
        internal static string HelpArgumentsTitle() =>
            GetResourceString(Properties.Resources.HelpArgumentsTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to Options:.
        /// </summary>
        internal static string HelpOptionsTitle() =>
            GetResourceString(Properties.Resources.HelpOptionsTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to (REQUIRED).
        /// </summary>
        internal static string HelpOptionsRequiredLabel() =>
            GetResourceString(Properties.Resources.HelpOptionsRequiredLabel);

        /// <summary>
        ///   Interpolates values into a localized string similar to default.
        /// </summary>
        internal static string HelpArgumentDefaultValueLabel() =>
            GetResourceString(Properties.Resources.HelpArgumentDefaultValueLabel);

        /// <summary>
        ///   Interpolates values into a localized string similar to Commands:.
        /// </summary>
        internal static string HelpCommandsTitle() =>
            GetResourceString(Properties.Resources.HelpCommandsTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to Additional Arguments:.
        /// </summary>
        internal static string HelpAdditionalArgumentsTitle() =>
            GetResourceString(Properties.Resources.HelpAdditionalArgumentsTitle);

        /// <summary>
        ///   Interpolates values into a localized string similar to Arguments passed to the application that is being run..
        /// </summary>
        internal static string HelpAdditionalArgumentsDescription() =>
            GetResourceString(Properties.Resources.HelpAdditionalArgumentsDescription);

        /// <summary>
        ///   Interpolates values into a localized string similar to &apos;{0}&apos; was not matched. Did you mean one of the following?.
        /// </summary>
        internal static string SuggestionsTokenNotMatched(string token)
            => GetResourceString(Properties.Resources.SuggestionsTokenNotMatched, token);

        /// <summary>
        ///   Interpolates values into a localized string similar to Show version information.
        /// </summary>
        internal static string VersionOptionDescription()
            => GetResourceString(Properties.Resources.VersionOptionDescription);

        /// <summary>
        ///   Interpolates values into a localized string similar to {0} option cannot be combined with other arguments..
        /// </summary>
        internal static string VersionOptionCannotBeCombinedWithOtherArguments(string optionAlias)
            => GetResourceString(Properties.Resources.VersionOptionCannotBeCombinedWithOtherArguments, optionAlias);

        /// <summary>
        ///   Interpolates values into a localized string similar to Unhandled exception: .
        /// </summary>
        internal static string ExceptionHandlerHeader()
            => GetResourceString(Properties.Resources.ExceptionHandlerHeader);

        /// <summary>
        ///   Interpolates values into a localized string similar to Cannot parse argument &apos;{0}&apos; as expected type {1}..
        /// </summary>
        internal static string ArgumentConversionCannotParse(string value, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParse, value, expectedType);

        /// <summary>
        ///   Interpolates values into a localized string similar to Cannot parse argument &apos;{0}&apos; for command &apos;{1}&apos; as expected type {2}..
        /// </summary>
        internal static string ArgumentConversionCannotParseForCommand(string value, string commandAlias, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForCommand, value, commandAlias, expectedType);

        /// <summary>
        ///   Interpolates values into a localized string similar to Cannot parse argument &apos;{0}&apos; for command &apos;{1}&apos; as expected type {2}..
        /// </summary>
        internal static string ArgumentConversionCannotParseForCommand(string value, string commandAlias, Type expectedType, IEnumerable<string> completions)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForCommand_Completions,
                value, commandAlias, expectedType, Environment.NewLine + string.Join(Environment.NewLine, completions));

        /// <summary>
        ///   Interpolates values into a localized string similar to Cannot parse argument &apos;{0}&apos; for option &apos;{1}&apos; as expected type {2}..
        /// </summary>
        internal static string ArgumentConversionCannotParseForOption(string value, string optionAlias, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForOption, value, optionAlias, expectedType);

        /// <summary>
        ///   Interpolates values into a localized string similar to Cannot parse argument &apos;{0}&apos; for option &apos;{1}&apos; as expected type {2}..
        /// </summary>
        internal static string ArgumentConversionCannotParseForOption(string value, string optionAlias, Type expectedType, IEnumerable<string> completions)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForOption_Completions,
                value, optionAlias, expectedType, Environment.NewLine + string.Join(Environment.NewLine, completions));

        /// <summary>
        /// Interpolates values into a localized string.
        /// </summary>
        /// <param name="resourceString">The string template into which values will be interpolated.</param>
        /// <param name="formatArguments">The values to interpolate.</param>
        /// <returns>The final string after interpolation.</returns>
        private static string GetResourceString(string resourceString, params object[] formatArguments)
        {
            if (resourceString is null)
            {
                return string.Empty;
            }
            if (formatArguments.Length > 0)
            {
                return string.Format(resourceString, formatArguments);
            }
            return resourceString;
        }

        private static string GetOptionName(OptionResult optionResult) => optionResult.IdentifierToken?.Value ?? optionResult.Option.Name;
    }
}
