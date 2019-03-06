// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;

namespace System.CommandLine.Builder
{
    public static class ArgumentExtensions
    {
        public static TArgument FromAmong<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.AddValidValues(values);
            argument.AddSuggestions(values);

            return argument;
        }

        public static TArgument WithSuggestions<TArgument>(
            this TArgument argument,
            params string[] suggestions)
            where TArgument : Argument
        {
            argument.AddSuggestions(suggestions);

            return argument;
        }

        public static TArgument WithSuggestionSource<TArgument>(
            this TArgument argument,
            Suggest suggest)
            where TArgument : Argument
        {
            argument.AddSuggestionSource(suggest);

            return argument;
        }

        public static Argument<FileInfo> ExistingOnly(this Argument<FileInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !File.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<DirectoryInfo> ExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !Directory.Exists(filePath))
                                            .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<FileInfo[]> ExistingOnly(this Argument<FileInfo[]> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !File.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument<DirectoryInfo[]> ExistingOnly(this Argument<DirectoryInfo[]> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !Directory.Exists(filePath))
                                            .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static TArgument LegalFilePathsOnly<TArgument>(
            this TArgument argument)
            where TArgument : Argument
        {
            argument.AddValidator(symbol =>
            {
                foreach (var arg in symbol.Arguments)
                {
                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = arg.IndexOfAny(Path.GetInvalidPathChars());

                    if (invalidCharactersIndex >= 0)
                    {
                        return symbol.ValidationMessages.InvalidCharactersInPath(arg[invalidCharactersIndex]);
                    }
                }

                return null;
            });

            return argument;
        }
    }
}
