// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Argument<T> : Argument
    {
        public Argument() : base(null)
        {
            ArgumentType = typeof(T);
        }

        public Argument(
            string name, 
            string description = null) : base(name)
        {
            ArgumentType = typeof(T);
            Description = description;
        }

        public Argument(
            string name, 
            Func<T> getDefaultValue, 
            string description = null) : this(name)
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());

            Description = description;
        }

        public Argument(Func<T> getDefaultValue) : this()
        {
            if (getDefaultValue is null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());
        }

        public Argument(
            string? name,
            ParseArgument<T> parse, 
            bool isDefault = false) : this()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
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
                    value = default(T);
                    return false;
                }
            };
        }

        public Argument(ParseArgument<T> parse, bool isDefault = false) : this(null, parse, isDefault)
        {
        }
    }
}
