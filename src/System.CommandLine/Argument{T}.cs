// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    ///<inheritdoc/>
    public class Argument<T> : Argument
    {
        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        public Argument()
        {
            ArgumentType = typeof(T);
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        public Argument(
            string name, 
            string? description = null) : base(name)
        {
            ArgumentType = typeof(T);
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the Argument.</param>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public Argument(
            string name, 
            Func<T> getDefaultValue, 
            string? description = null) : this(name)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());

            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public Argument(Func<T> getDefaultValue) : this()
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><c>true</c> to use parser result as default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parse"/> is null.</exception>
        public Argument(
            string? name,
            ParseArgument<T> parse, 
            bool isDefault = false) : this()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name!;
            }

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
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><c>true</c> to use parser result as default value.</param>
        public Argument(ParseArgument<T> parse, bool isDefault = false) : this(null, parse, isDefault)
        {
        }
    }
}
