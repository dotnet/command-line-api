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
