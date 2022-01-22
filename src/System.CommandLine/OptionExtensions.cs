// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Option" />.
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Configures an option to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="values">The values that are allowed for the option.</param>
        /// <typeparam name="TOption">The type of the option's parsed value.</typeparam>
        /// <returns>The configured argument.</returns>
        public static TOption FromAmong<TOption>(
            this TOption option,
            params string[] values)
            where TOption : Option
        {
            option.Argument.AddAllowedValues(values);
            option.Argument.Completions.Add(values);

            return option;
        }

        /// <summary>
        /// Adds completions for an option.
        /// </summary>
        /// <typeparam name="TOption">The type of the <see cref="Option" />.</typeparam>
        /// <param name="option">The option for which to add completions.</param>
        /// <param name="values">The completions to add.</param>
        /// <returns>The option being extended.</returns>
        public static TOption AddCompletions<TOption>(
            this TOption option,
            params string[] values)
            where TOption : Option
        {
            option.Argument.Completions.Add(values);

            return option;
        }
        
        /// <summary>
        /// Adds completions for an option.
        /// </summary>
        /// <typeparam name="TOption">The type of the option.</typeparam>
        /// <param name="option">The option for which to add completions.</param>
        /// <param name="complete">A <see cref="CompletionDelegate"/> that will be called to provide completions.</param>
        /// <returns>The option being extended.</returns>
        public static TOption AddCompletions<TOption>(
            this TOption option,
            Func<CompletionContext, IEnumerable<string>> complete)
            where TOption : Option
        {
            option.Argument.Completions.Add(complete);

            return option;
        }
   
        /// <summary>
        /// Adds completions for an option.
        /// </summary>
        /// <typeparam name="TOption">The type of the option.</typeparam>
        /// <param name="option">The option for which to add completions.</param>
        /// <param name="complete">A <see cref="CompletionDelegate"/> that will be called to provide completions.</param>
        /// <returns>The option being extended.</returns>
        public static TOption AddCompletions<TOption>(
            this TOption option,
            CompletionDelegate complete)
            where TOption : Option
        {
            option.Argument.Completions.Add(complete);

            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<FileInfo> ExistingOnly(this Option<FileInfo> option)
        {
            option.Argument.AddValidator(Validate.FileExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<DirectoryInfo> ExistingOnly(this Option<DirectoryInfo> option)
        {
            option.Argument.AddValidator(Validate.DirectoryExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<FileSystemInfo> ExistingOnly(this Option<FileSystemInfo> option)
        {
            option.Argument.AddValidator(Validate.FileOrDirectoryExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<T> ExistingOnly<T>(this Option<T> option)
            where T : IEnumerable<FileSystemInfo>
        {
            if (option.Argument is Argument<T> arg)
            {
                arg.ExistingOnly();
            }

            return option;
        }

        /// <summary>
        /// Configures an option to accept only values representing legal file paths.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static TOption LegalFilePathsOnly<TOption>(
            this TOption option)
            where TOption : Option
        {
            option.Argument.LegalFilePathsOnly();

            return option;
        }

        /// <summary>
        /// Configures an option to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static TOption LegalFileNamesOnly<TOption>(
            this TOption option)
            where TOption : Option
        {
            option.Argument.LegalFileNamesOnly();

            return option;
        }

        /// <summary>
        /// Parses a command line string value using an option.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="option">The option to use to parse the command line input.</param>
        /// <param name="commandLine">A command line string to parse, which can include spaces and quotes equivalent to what can be entered into a terminal.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Option option,
            string commandLine) =>
            option.GetOrCreateDefaultSimpleParser().Parse(commandLine);

        /// <summary>
        /// Parses a command line string value using an option.
        /// </summary>
        /// <param name="option">The option to use to parse the command line input.</param>
        /// <param name="args">The string options to parse.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Option option,
            string[] args) =>
            option.GetOrCreateDefaultSimpleParser().Parse(args);
    }
}