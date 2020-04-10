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

        /// <summary>
        /// Helper method to locate an <see cref="IOption"/> based on a provided alias
        /// </summary>
        /// <param name="options">the list of IOptions to search</param>
        /// <param name="alias">the alias to search for</param>
        /// <returns>the first <see cref="IOption"/> defining an alias matching the provided one</returns>
        /// <exception cref="UnknownAliasException">
        /// thrown if no matching <see cref="IOption"/> could be found
        /// </exception>
        public static IOption FindFirstMatch( this List<IOption> options, string alias )
        {
            if( string.IsNullOrEmpty(alias))
                throw new NullReferenceException($"{nameof(alias)} is undefined or empty");

            var retVal = options
                .Find( x => x.Aliases.FirstOrDefault( a => a == alias ) != null );

            if( retVal == null )
                throw new UnknownAliasException( alias, true );

            return retVal;
        }
    }
}
