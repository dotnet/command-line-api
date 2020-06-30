// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.IO;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    public static class ArgumentExtensions
    {
        public static TArgument FromAmong<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.AddAllowedValues(values);
            argument.SuggestionSources.Add(values);

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
            argument.AddValidator(symbol =>
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
