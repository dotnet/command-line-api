﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine
{
    /// <inheritdoc cref="Argument" />
    public class Argument<T> : Argument, IValueDescriptor<T>
    {
        private readonly bool _hasCustomParser;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        public Argument()
        {
        }

        /// <inheritdoc />
        public Argument(
            string? name, 
            string? description = null) : base(name, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="defaultValueFactory">The delegate to invoke to return the default value.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="defaultValueFactory"/> is null.</exception>
        public Argument(
            string name, 
            Func<T> defaultValueFactory, 
            string? description = null) : this(name, description)
        {
            if (defaultValueFactory is null)
            {
                throw new ArgumentNullException(nameof(defaultValueFactory));
            }

            SetDefaultValueFactory(() => defaultValueFactory());
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="defaultValueFactory">The delegate to invoke to return the default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="defaultValueFactory"/> is null.</exception>
        public Argument(Func<T> defaultValueFactory) : this()
        {
            if (defaultValueFactory is null)
            {
                throw new ArgumentNullException(nameof(defaultValueFactory));
            }

            SetDefaultValueFactory(() => defaultValueFactory());
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><see langword="true"/> to use the <paramref name="parse"/> result as default value.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parse"/> is null.</exception>
        public Argument(
            string? name,
            Func<ArgumentResult, T> parse, 
            bool isDefault = false,
            string? description = null) : this(name, description)
        {
            if (parse is null)
            {
                throw new ArgumentNullException(nameof(parse));
            }

            if (isDefault)
            {
                SetDefaultValueFactory(argumentResult => parse(argumentResult));
            }

            ConvertArguments = (ArgumentResult argumentResult, out object? value) =>
            {
                var result = parse(argumentResult);

                if (string.IsNullOrEmpty(argumentResult.ErrorMessage))
                {
                    value = result;
                    return true;
                }
                else
                {
                    value = default(T)!;
                    return false;
                }
            };

            _hasCustomParser = true;
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><see langword="true"/> to use the <paramref name="parse"/> result as default value.</param>
        public Argument(Func<ArgumentResult, T> parse, bool isDefault = false) : this(null!, parse, isDefault)
        {
        }

        internal override bool HasCustomParser => _hasCustomParser;

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        /// <summary>
        /// Adds completions for the argument.
        /// </summary>
        /// <param name="completions">The completions to add.</param>
        /// <returns>The configured argument.</returns>
        public Argument<T> AddCompletions(params string[] completions)
        {
            Completions.Add(completions);
            return this;
        }

        /// <summary>
        /// Adds completions for the argument.
        /// </summary>
        /// <param name="completionsDelegate">A function that will be called to provide completions.</param>
        /// <returns>The option being extended.</returns>
        public Argument<T> AddCompletions(Func<CompletionContext, IEnumerable<string>> completionsDelegate)
        {
            Completions.Add(completionsDelegate);
            return this;
        }

        /// <summary>
        /// Adds completions for the argument.
        /// </summary>
        /// <param name="completionsDelegate">A function that will be called to provide completions.</param>
        /// <returns>The configured argument.</returns>
        public Argument<T> AddCompletions(Func<CompletionContext, IEnumerable<CompletionItem>> completionsDelegate)
        {
            Completions.Add(completionsDelegate);
            return this;
        }

        /// <summary>
        /// Configures the argument to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="values">The values that are allowed for the argument.</param>
        /// <returns>The configured argument.</returns>
        public Argument<T> AcceptOnlyFromAmong(params string[] values)
        {
            AllowedValues?.Clear();
            AddAllowedValues(values);
            Completions.Clear();
            Completions.Add(values);

            return this;
        }

        /// <summary>
        /// Configures the argument to accept only values representing legal file paths.
        /// </summary>
        /// <returns>The configured argument.</returns>
        public Argument<T> AcceptLegalFilePathsOnly()
        {
            var invalidPathChars = Path.GetInvalidPathChars();

            AddValidator(result =>
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

            return this;
        }

        /// <summary>
        /// Configures the argument to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        /// <returns>The configured argument.</returns>
        public Argument<T> AcceptLegalFileNamesOnly()
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            AddValidator(result =>
            {
                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidFileNameChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.ErrorMessage = result.LocalizationResources.InvalidCharactersInFileName(token.Value[invalidCharactersIndex]);
                    }
                }
            });

            return this;
        }
    }
}
