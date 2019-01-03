// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Builder
{
    public static class ArgumentExtensions
    {
        public static Argument FromAmong(
            this Argument argument,
            params string[] values)
        {
            argument.AddValidValues(values);
            argument.AddSuggestions(values);

            return argument;
        }

        public static Argument WithSuggestions(
            this Argument argument,
            params string[] suggestions)
        {
            argument.AddSuggestions(suggestions);

            return argument;
        }

        public static Argument WithSuggestionSource(
            this Argument argument,
            Suggest suggest)
        {
            argument.AddSuggestionSource(suggest);

            return argument;
        }

        public static Argument ExistingOnly(this Argument<FileInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !File.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument ExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !Directory.Exists(filePath))
                                            .Select(symbol.ValidationMessages.DirectoryDoesNotExist)
                                            .FirstOrDefault());
            return argument;
        }

        public static Argument LegalFilePathsOnly(this Argument argument)
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
