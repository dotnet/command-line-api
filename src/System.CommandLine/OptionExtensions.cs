// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public static class OptionExtensions
    {
        public static TOption FromAmong<TOption>(
            this TOption option,
            params string[] values)
            where TOption : Option
        {
            option.Argument.AddAllowedValues(values);
            option.Argument.AddSuggestions(values);

            return option;
        }

        public static TOption WithSuggestions<TOption>(
            this TOption option,
            params string[] suggestions)
            where TOption : Option
        {
            option.Argument.AddSuggestions(suggestions);

            return option;
        }

        public static TOption WithSuggestionSource<TOption>(
            this TOption option,
            Suggest suggest)
            where TOption : Option
        {
            option.Argument.AddSuggestionSource(suggest);

            return option;
        }

        // FIX: (OptionExtensions) reduce/generalize ExistingOnly overloads

        public static Option<FileInfo> ExistingOnly(this Option<FileInfo> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !File.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.FileDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static Option<DirectoryInfo> ExistingOnly(this Option<DirectoryInfo> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !Directory.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static Option<FileSystemInfo> ExistingOnly(this Option<FileSystemInfo> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !Directory.Exists(filePath) && !File.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.FileOrDirectoryDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static Option<FileInfo[]> ExistingOnly(this Option<FileInfo[]> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !File.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.FileDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static Option<DirectoryInfo[]> ExistingOnly(this Option<DirectoryInfo[]> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !Directory.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static Option<FileSystemInfo[]> ExistingOnly(this Option<FileSystemInfo[]> option)
        {
            option.Argument.AddValidator(symbol =>
                                             symbol.Tokens
                                                   .Select(t => t.Value)
                                                   .Where(filePath => !Directory.Exists(filePath) && !File.Exists(filePath))
                                                   .Select(symbol.ValidationMessages.FileOrDirectoryDoesNotExist)
                                                   .FirstOrDefault());
            return option;
        }

        public static TOption LegalFilePathsOnly<TOption>(
            this TOption option)
            where TOption : Option
        {
            option.Argument.AddValidator(symbol =>
            {
                foreach (var token in symbol.Tokens)
                {
                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = token.Value.IndexOfAny(Path.GetInvalidPathChars());

                    if (invalidCharactersIndex >= 0)
                    {
                        return symbol.ValidationMessages.InvalidCharactersInPath(token.Value[invalidCharactersIndex]);
                    }
                }

                return null;
            });

            return option;
        }

        public static ParseResult Parse(
            this Option option,
            string commandLine) =>
            new Parser(new CommandLineConfiguration(new[] { option })).Parse(commandLine);
    }
}
