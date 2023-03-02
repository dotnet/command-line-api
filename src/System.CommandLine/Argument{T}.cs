// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine
{
    /// <inheritdoc cref="Argument" />
    public class Argument<T> : Argument, IValueDescriptor<T>
    {
        private Func<ArgumentResult, T>? _customParser;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>>
        public Argument(string name) : base(name)
        {
        }

        /// <summary>
        /// The delegate to invoke to return the default value.
        /// </summary>
        public Func<ArgumentResult, T>? DefaultValueFactory { get; set; }

        /// <summary>
        /// A custom argument parser.
        /// </summary>
        public Func<ArgumentResult, T>? CustomParser
        {
            get => _customParser;
            set
            {
                _customParser = value;

                if (value is not null)
                {
                    // TODO: remove the following code or move it to the parsing logic
                    ConvertArguments = (ArgumentResult argumentResult, out object? parsedValue) =>
                    {
                        int errorsBefore = argumentResult.SymbolResultTree.ErrorCount;
                        var result = value(argumentResult);

                        if (errorsBefore == argumentResult.SymbolResultTree.ErrorCount)
                        {
                            parsedValue = result;
                            return true;
                        }
                        else
                        {
                            parsedValue = default(T)!;
                            return false;
                        }
                    };
                }
            }
        }

        // TODO: try removing it
        internal override bool HasCustomParser => _customParser is not null;

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        // TODO: try removing it, or at least make it internal
        /// <inheritdoc />
        public override bool HasDefaultValue => DefaultValueFactory is not null;

        internal override object? GetDefaultValue(ArgumentResult argumentResult)
        {
            if (DefaultValueFactory is null)
            {
                throw new InvalidOperationException($"Argument \"{Name}\" does not have a default value");
            }

            return DefaultValueFactory.Invoke(argumentResult);
        }

        /// <summary>
        /// Configures the argument to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="values">The values that are allowed for the argument.</param>
        public void AcceptOnlyFromAmong(params string[] values)
        {
            if (values is not null && values.Length > 0)
            {
                Validators.Clear();
                Validators.Add(UnrecognizedArgumentError);
                CompletionSources.Clear();
                CompletionSources.Add(values);
            }

            void UnrecognizedArgumentError(ArgumentResult argumentResult)
            {
                for (var i = 0; i < argumentResult.Tokens.Count; i++)
                {
                    var token = argumentResult.Tokens[i];

                    if (token.Symbol is null || token.Symbol == this)
                    {
                        if (Array.IndexOf(values, token.Value) < 0)
                        {
                            argumentResult.AddError(LocalizationResources.UnrecognizedArgument(token.Value, values));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configures the argument to accept only values representing legal file paths.
        /// </summary>
        public void AcceptLegalFilePathsOnly()
        {
            Validators.Add(static result =>
            {
                var invalidPathChars = Path.GetInvalidPathChars();

                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];

                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidPathChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.AddError(LocalizationResources.InvalidCharactersInPath(token.Value[invalidCharactersIndex]));
                    }
                }
            });
        }

        /// <summary>
        /// Configures the argument to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        public void AcceptLegalFileNamesOnly()
        {
            Validators.Add(static result =>
            {
                var invalidFileNameChars = Path.GetInvalidFileNameChars();

                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidFileNameChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.AddError(LocalizationResources.InvalidCharactersInFileName(token.Value[invalidCharactersIndex]));
                    }
                }
            });
        }
    }
}
