// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <inheritdoc cref="Option" />
    /// <typeparam name="T">The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.</typeparam>
    public class Option<T> : Option, IValueDescriptor<T>
    {
        /// <inheritdoc/>
        public Option(
            string name,
            string? description = null) 
            : base(name, description, new Argument<T>())
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            string? description = null) 
            : base(aliases, description, new Argument<T>())
        { }

        /// <inheritdoc/>
        public Option(
            string name,
            Func<ArgumentResult, T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(name, description, 
                  new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            Func<ArgumentResult, T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(aliases, description, new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        /// <inheritdoc/>
        public Option(
            string name,
            Func<T> defaultValueFactory,
            string? description = null) 
            : base(name, description, 
                  new Argument<T>(defaultValueFactory ?? throw new ArgumentNullException(nameof(defaultValueFactory))))
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            Func<T> defaultValueFactory,
            string? description = null)
            : base(aliases, description, new Argument<T>(defaultValueFactory ?? throw new ArgumentNullException(nameof(defaultValueFactory))))
        {
        }

        /// <summary>
        /// Configures the option to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="values">The values that are allowed for the option.</param>
        /// <returns>The configured option.</returns>
        public Option<T> AcceptOnlyFromAmong(params string[] values)
        {
            Argument.AcceptOnlyFromAmong(values);

            return this;
        }

        /// <summary>
        /// Adds completions for the option.
        /// </summary>
        /// <param name="completions">The completions to add.</param>
        /// <returns>The configured option.</returns>
        public Option<T> AddCompletions(params string[] completions)
        {
            Argument.Completions.Add(completions);
            return this;
        }

        /// <summary>
        /// Adds completions for the option.
        /// </summary>
        /// <param name="completionsDelegate">A function that will be called to provide completions.</param>
        /// <returns>The configured option.</returns>
        public Option<T> AddCompletions(Func<CompletionContext, IEnumerable<string>> completionsDelegate)
        {
            Argument.Completions.Add(completionsDelegate);
            return this;
        }

        /// <summary>
        /// Adds completions for the option.
        /// </summary>
        /// <param name="completionsDelegate">A function that will be called to provide completions.</param>
        /// <returns>The configured option.</returns>
        public Option<T> AddCompletions(Func<CompletionContext, IEnumerable<CompletionItem>> completionsDelegate)
        {
            Argument.Completions.Add(completionsDelegate);
            return this;
        }

        /// <summary>
        /// Configures the option to accept only values representing legal file paths.
        /// </summary>
        /// <returns>The configured option.</returns>
        public Option<T> AcceptLegalFilePathsOnly()
        {
            Argument.AcceptLegalFilePathsOnly();
            return this;
        }

        /// <summary>
        /// Configures the option to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        /// <returns>The configured option.</returns>
        public Option<T> AcceptLegalFileNamesOnly()
        {
            Argument.AcceptLegalFileNamesOnly();
            return this;
        }
    }
}