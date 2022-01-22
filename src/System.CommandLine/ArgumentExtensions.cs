// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Argument" />.
    /// </summary>
    public static class ArgumentExtensions
    {
        /// <summary>
        /// Adds completions for an argument.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="argument">The argument for which to add completions.</param>
        /// <param name="values">The completions to add.</param>
        /// <returns>The configured argument.</returns>
        public static TArgument AddCompletions<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.Completions.Add(values);

            return argument;
        }
    
        /// <summary>
        /// Adds completions for an option.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="argument">The argument for which to add completions.</param>
        /// <param name="complete">A <see cref="CompletionDelegate"/> that will be called to provide completions.</param>
        /// <returns>The option being extended.</returns>
        public static TArgument AddCompletions<TArgument>(
            this TArgument argument,
            Func<CompletionContext, IEnumerable<string>> complete)
            where TArgument : Argument
        {
            argument.Completions.Add(complete);

            return argument;
        }

        /// <summary>
        /// Adds completions for an argument.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="argument">The argument for which to add completions.</param>
        /// <param name="complete">A <see cref="CompletionDelegate"/> that will be called to provide completions.</param>
        /// <returns>The configured argument.</returns>
        public static TArgument AddCompletions<TArgument>(
            this TArgument argument,
            CompletionDelegate complete)
            where TArgument : Argument
        {
            argument.Completions.Add(complete);

            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="values">The values that are allowed for the argument.</param>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <returns>The configured argument.</returns>
        public static TArgument FromAmong<TArgument>(
            this TArgument argument,
            params string[] values)
            where TArgument : Argument
        {
            argument.AddAllowedValues(values);
            argument.Completions.Add(values);

            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<FileInfo> ExistingOnly(this Argument<FileInfo> argument)
        {
            argument.AddValidator(Validate.FileExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<DirectoryInfo> ExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.AddValidator(Validate.DirectoryExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<FileSystemInfo> ExistingOnly(this Argument<FileSystemInfo> argument)
        {
            argument.AddValidator(Validate.FileOrDirectoryExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<T> ExistingOnly<T>(this Argument<T> argument)
            where T : IEnumerable<FileSystemInfo>
        {
            if (typeof(IEnumerable<FileInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(Validate.FileExists);
            }
            else if (typeof(IEnumerable<DirectoryInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(Validate.DirectoryExists);
            }
            else
            {
                argument.AddValidator(Validate.FileOrDirectoryExists);
            }

            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values representing legal file paths.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static TArgument LegalFilePathsOnly<TArgument>(
            this TArgument argument)
            where TArgument : Argument
        {
            var invalidPathChars = Path.GetInvalidPathChars();

            argument.AddValidator(result =>
            {
                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];

                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidPathChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.ErrorMessage = result.LocalizationResources.InvalidCharactersInPath(token.Value[invalidCharactersIndex]);
                    }
                }
            });

            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static TArgument LegalFileNamesOnly<TArgument>(
            this TArgument argument)
            where TArgument : Argument
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            argument.AddValidator(result =>
            {
                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidFileNameChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.ErrorMessage =  result.LocalizationResources.InvalidCharactersInFileName(token.Value[invalidCharactersIndex]);
                    }
                }
            });

            return argument;
        }
        
        /// <summary>
        /// Parses a command line string value using an argument.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="argument">The argument to use to parse the command line input.</param>
        /// <param name="commandLine">A command line string to parse, which can include spaces and quotes equivalent to what can be entered into a terminal.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Argument argument,
            string commandLine) =>
            argument.GetOrCreateDefaultSimpleParser().Parse(commandLine);

        /// <summary>
        /// Parses a command line string value using an argument.
        /// </summary>
        /// <param name="argument">The argument to use to parse the command line input.</param>
        /// <param name="args">The string arguments to parse.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Argument argument,
            string[] args) =>
            argument.GetOrCreateDefaultSimpleParser().Parse(args);
    }
}
