// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
            option.Argument.Suggestions.Add(values);

            return option;
        }

        public static TOption AddSuggestions<TOption>(
            this TOption option,
            params string[] values)
            where TOption : Option
        {
            option.Argument.Suggestions.Add(values);

            return option;
        }

        public static TOption AddSuggestions<TOption>(
            this TOption option,
            SuggestDelegate suggest)
            where TOption : Option 
        {
            option.Argument.Suggestions.Add(suggest);

            return option;
        }

        public static Option<FileInfo> ExistingOnly(this Option<FileInfo> option)
        {
            option.Argument.AddValidator(
                a =>
                    a.Tokens
                     .Select(t => t.Value)
                     .Where(filePath => !File.Exists(filePath))
                     .Select(a.ValidationMessages.FileDoesNotExist)
                     .FirstOrDefault());

            return option;
        }

        public static Option<DirectoryInfo> ExistingOnly(this Option<DirectoryInfo> option)
        {
            option.Argument.AddValidator(
                a =>
                    a.Tokens
                     .Select(t => t.Value)
                     .Where(filePath => !Directory.Exists(filePath))
                     .Select(a.ValidationMessages.DirectoryDoesNotExist)
                     .FirstOrDefault());

            return option;
        }

        public static Option<FileSystemInfo> ExistingOnly(this Option<FileSystemInfo> option)
        {
            option.Argument.AddValidator(
                a =>
                    a.Tokens
                     .Select(t => t.Value)
                     .Where(filePath => !Directory.Exists(filePath) && !File.Exists(filePath))
                     .Select(a.ValidationMessages.FileOrDirectoryDoesNotExist)
                     .FirstOrDefault());

            return option;
        }

        public static Option<T> ExistingOnly<T>(this Option<T> option)
            where T : IEnumerable<FileSystemInfo>
        {
            if (option.Argument is Argument<T> arg)
            {
                arg.ExistingOnly();
            }

            return option;
        }

        public static TOption LegalFilePathsOnly<TOption>(
            this TOption option)
            where TOption : Option
        {
            option.Argument.LegalFilePathsOnly();

            return option;
        }

        public static ParseResult Parse(
            this Option option,
            string commandLine) =>
            new Parser(new CommandLineConfiguration(new[] { option })).Parse(commandLine);
    }
}
