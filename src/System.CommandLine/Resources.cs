// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public class Resources
    {
        public static Resources Instance { get; } = new Resources();

        protected Resources()
        {
        }

        public virtual string ExpectsOneArgument(SymbolResult symbolResult) => 
            symbolResult is CommandResult
                    ? GetResourceString(Properties.Resources.CommandExpectsOneArgument, symbolResult.Token().Value, symbolResult.Tokens.Count)
                    : GetResourceString(Properties.Resources.OptionExpectsOneArgument, symbolResult.Token().Value, symbolResult.Tokens.Count);

        public virtual string NoArgumentProvided(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? GetResourceString(Properties.Resources.CommandNoArgumentProvided, symbolResult.Token().Value)
                : GetResourceString(Properties.Resources.OptionNoArgumentProvided, symbolResult.Token().Value);

        public virtual string ExpectsFewerArguments(
            Token token,
            int providedNumberOfValues,
            int maximumNumberOfValues) =>
            token.Type == TokenType.Command
                ? GetResourceString(Properties.Resources.CommandExpectsFewerArguments, token, maximumNumberOfValues, providedNumberOfValues)
                : GetResourceString(Properties.Resources.OptionExpectsFewerArguments, token, maximumNumberOfValues, providedNumberOfValues);
                
        public virtual string DirectoryDoesNotExist(string path) =>
            GetResourceString(Properties.Resources.DirectoryDoesNotExist, path);

        public virtual string FileDoesNotExist(string filePath) =>
            GetResourceString(Properties.Resources.FileDoesNotExist, filePath);

        public virtual string FileOrDirectoryDoesNotExist(string path) =>
            GetResourceString(Properties.Resources.FileOrDirectoryDoesNotExist, path);

        public virtual string InvalidCharactersInPath(char invalidChar) =>
            GetResourceString(Properties.Resources.InvalidCharactersInPath, invalidChar);

        public virtual string InvalidCharactersInFileName(char invalidChar) =>
            GetResourceString(Properties.Resources.InvalidCharactersInFileName, invalidChar);

        public virtual string RequiredArgumentMissing(SymbolResult symbolResult) =>
            symbolResult is CommandResult
                ? GetResourceString(Properties.Resources.CommandRequiredArgumentMissing, symbolResult.Token().Value)
                : GetResourceString(Properties.Resources.OptionRequiredArgumentMissing, symbolResult.Token().Value);

        public virtual string RequiredCommandWasNotProvided() =>
            GetResourceString(Properties.Resources.RequiredCommandWasNotProvided);

        public virtual string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) =>
            GetResourceString(Properties.Resources.UnrecognizedArgument, unrecognizedArg,$"\n\t{string.Join("\n\t", allowedValues.Select(v => $"'{v}'"))}");

        public virtual string UnrecognizedCommandOrArgument(string arg) =>
            GetResourceString(Properties.Resources.UnrecognizedCommandOrArgument, arg);

        public virtual string ResponseFileNotFound(string filePath) =>
            GetResourceString(Properties.Resources.ResponseFileNotFound, filePath);

        public virtual string ErrorReadingResponseFile(string filePath, IOException e) =>
            GetResourceString(Properties.Resources.ErrorReadingResponseFile, filePath, e.Message);

        public virtual string HelpOptionDescription() =>
            GetResourceString(Properties.Resources.HelpOptionDescription);

        public virtual string HelpUsageTile() =>
            GetResourceString(Properties.Resources.HelpUsageTile);

        public virtual string HelpDescriptionTitle() =>
            GetResourceString(Properties.Resources.HelpDescriptionTitle);

        public virtual string HelpUsageOptionsTile() =>
            GetResourceString(Properties.Resources.HelpUsageOptionsTitle);

        public virtual string HelpUsageCommandTile() =>
            GetResourceString(Properties.Resources.HelpUsageCommandTitle);

        public virtual string HelpUsageAdditionalArguments() =>
            GetResourceString(Properties.Resources.HelpUsageAdditionalArguments);

        public virtual string HelpArgumentsTitle() =>
            GetResourceString(Properties.Resources.HelpArgumentsTitle);

        public virtual string HelpOptionsTitle() =>
            GetResourceString(Properties.Resources.HelpOptionsTitle);

        public virtual string HelpOptionsRequired() =>
            GetResourceString(Properties.Resources.HelpOptionsRequired);

        public virtual string HelpArgumentDefaultValueTitle() =>
            GetResourceString(Properties.Resources.HelpArgumentDefaultValueTitle);

        public virtual string HelpCommandsTitle() =>
            GetResourceString(Properties.Resources.HelpCommandsTitle);

        public virtual string HelpAdditionalArgumentsTitle() =>
            GetResourceString(Properties.Resources.HelpAdditionalArgumentsTitle);

        public virtual string HelpAdditionalArgumentsDescription() =>
            GetResourceString(Properties.Resources.HelpAdditionalArgumentsDescription);

        public virtual string SuggestionsTokenNotMatched(string token)
            => GetResourceString(Properties.Resources.SuggestionsTokenNotMatched, token);

        public virtual string VersionOptionDescription()
            => GetResourceString(Properties.Resources.VersionOptionDescription);

        public virtual string VersionOptionCannotBeCombinedWithOtherArguments(string optionAlias)
            => GetResourceString(Properties.Resources.VersionOptionCannotBeCombinedWithOtherArguments, optionAlias);

        public virtual string ExceptionHandlerHeader()
            => GetResourceString(Properties.Resources.ExceptionHandlerHeader);

        public virtual string DebugDirectiveExecutableNotSpecified(string environmentVariableName, string processName)
            => GetResourceString(Properties.Resources.DebugDirectiveExecutableNotSpecified, environmentVariableName, processName);

        public virtual string DebugDirectiveAttachToProcess(int processId, string processName)
            => GetResourceString(Properties.Resources.DebugDirectiveAttachToProcess, processId, processName);

        public virtual string DebugDirectiveProcessNotIncludedInEnvironmentVariable(string processName, string environmentVariableName, string processNames)
            => GetResourceString(Properties.Resources.DebugDirectiveProcessNotIncludedInEnvironmentVariable, processName, environmentVariableName, processNames);

        public virtual string DotnetSuggestExceptionOccurred(Exception exception)
            => GetResourceString(Properties.Resources.DotnetSuggestExceptionOccurred, exception);

        public virtual string DotnetSuggestExitMessage(string dotnetSuggestName, int exitCode, string standardOut, string standardError)
            => GetResourceString(Properties.Resources.DotnetSuggestExitMessage, dotnetSuggestName, exitCode, standardOut, standardError);

        public virtual string ArgumentConversionCannotParse(string value, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParse, value, expectedType);

        public virtual string ArgumentConversionCannotParseForCommand(string value, string commandAlias, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForCommand, value, commandAlias, expectedType);

        public virtual string ArgumentConversionCannotParseForOption(string value, string optionAlias, Type expectedType)
            => GetResourceString(Properties.Resources.ArgumentConversionCannotParseForOption, value, optionAlias, expectedType);

        protected virtual string GetResourceString(string resourceString, params object[] formatArguments)
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
    }
}
