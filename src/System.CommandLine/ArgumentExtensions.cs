// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;
using System.IO;

namespace System.CommandLine
{
    public static class ArgumentExtensions
    {
        public static TArgument AddSuggestions<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.Suggestions.Add(values);

            return argument;
        }

        public static TArgument AddSuggestions<TArgument>(
            this TArgument argument,
            SuggestDelegate suggest)
            where TArgument : Argument
        {
            argument.Suggestions.Add(suggest);

            return argument;
        }

        public static TArgument FromAmong<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.AddAllowedValues(values);
            argument.Suggestions.Add(values);

            return argument;
        }

        public static Argument<FileInfo> ExistingOnly(this Argument<FileInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Tokens
                                            .Select(t => t.Value)
                                            .Where(filePath => !File.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<DirectoryInfo> ExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Tokens
                                            .Select(t => t.Value)
                                            .Where(filePath => !Directory.Exists(filePath))
                                            .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<FileSystemInfo> ExistingOnly(this Argument<FileSystemInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Tokens
                                            .Select(t => t.Value)
                                            .Where(filePath => !Directory.Exists(filePath) && !File.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileOrDirectoryDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<T> ExistingOnly<T>(this Argument<T> argument)
            where T : IEnumerable<FileSystemInfo>
        {
            if (typeof(IEnumerable<FileInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(
                    a => a.Tokens
                          .Select(t => t.Value)
                          .Where(filePath => !File.Exists(filePath))
                          .Select(a.ValidationMessages.FileDoesNotExist)
                          .FirstOrDefault());
            }
            else if (typeof(IEnumerable<DirectoryInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(
                    a => a.Tokens
                          .Select(t => t.Value)
                          .Where(filePath => !Directory.Exists(filePath))
                          .Select(a.ValidationMessages.DirectoryDoesNotExist)
                          .FirstOrDefault());
            }
            else
            {
                argument.AddValidator(
                    a => a.Tokens
                          .Select(t => t.Value)
                          .Where(filePath => !Directory.Exists(filePath) && !File.Exists(filePath))
                          .Select(a.ValidationMessages.FileOrDirectoryDoesNotExist)
                          .FirstOrDefault());
            }

            return argument;
        }

        public static TArgument LegalFilePathsOnly<TArgument>(
            this TArgument argument)
            where TArgument : Argument
        {
            var invalidPathChars = Path.GetInvalidPathChars();

            argument.AddValidator(symbol =>
            {
                for (var i = 0; i < symbol.Tokens.Count; i++)
                {
                    var token = symbol.Tokens[i];

                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidPathChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        return symbol.ValidationMessages.InvalidCharactersInPath(token.Value[invalidCharactersIndex]);
                    }
                }

                return null;
            });

            return argument;
        }

        public static TArgument LegalFileNamesOnly<TArgument>(
            this TArgument argument)
            where TArgument : Argument
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            argument.AddValidator(symbol =>
            {
                for (var i = 0; i < symbol.Tokens.Count; i++)
                {
                    var token = symbol.Tokens[i];
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidFileNameChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        return symbol.ValidationMessages.InvalidCharactersInFileName(token.Value[invalidCharactersIndex]);
                    }
                }

                return null;
            });

            return argument;
        }

        public static ParseResult Parse(
            this Argument argument,
            string commandLine) =>
            new Parser(
                new CommandLineConfiguration(
                    new[]
                    {
                        new RootCommand
                        {
                            argument
                        },
                    })).Parse(commandLine);
    }
}
