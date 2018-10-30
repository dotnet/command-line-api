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

        public static Argument WithDefaultValue(
            this Argument argument,
            Func<object> defaultValue)
        {
            argument.SetDefaultValue(defaultValue);

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

        public static void ExistingFilesOnly(this Argument argument) =>
            argument.AddValidator(symbol =>
                                      symbol.Arguments
                                            .Where(filePath => !File.Exists(filePath) &&
                                                               !Directory.Exists(filePath))
                                            .Select(symbol.ValidationMessages.FileDoesNotExist)
                                            .FirstOrDefault());

        public static Argument LegalFilePathsOnly(this Argument argument)
        {
            argument.AddValidator(symbol =>
            {
                var errorMessage = new List<(string, string)>();

                foreach (var arg in symbol.Arguments)
                {
                    try
                    {
                        var fileInfo = new FileInfo(arg);
                    }
                    catch (NotSupportedException ex)
                    {
                        errorMessage.Add((arg, ex.Message));
                    }
                    catch (ArgumentException ex)
                    {
                        errorMessage.Add((arg, ex.Message));
                    }

                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = arg.IndexOfAny(Path.GetInvalidPathChars());
                    if (invalidCharactersIndex >= 0)
                    {
                        errorMessage.Add((arg, arg[invalidCharactersIndex] + " is invalid character in path {arg}"));
                    }
                }

                if (errorMessage.Any())
                {
                    return errorMessage
                           .Select(e => $"Argument {e.Item1} failed validation due to {e.Item2}")
                           .Aggregate((current, next) => current + Environment.NewLine + next);
                }

                return null;
            });

            return argument;
        }
    }
}
