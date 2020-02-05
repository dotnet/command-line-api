// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Argument<T> : Argument
    {
        public Argument(string name) : this()
        {
            Name = name;
        }

        public Argument() : base(null)
        {
            ArgumentType = typeof(T);
        }

        public Argument(string name, Func<T> getDefaultValue) : this(name)
        {
            if (getDefaultValue == null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());
        }

        public Argument(Func<T> getDefaultValue) : this()
        {
            if (getDefaultValue == null)
            {
                throw new ArgumentNullException(nameof(getDefaultValue));
            }

            SetDefaultValueFactory(() => getDefaultValue());
        }

        public Argument(TryConvertArgument<T> convert, Func<T> getDefaultValue = default) : this()
        {
            if (convert == null)
            {
                throw new ArgumentNullException(nameof(convert));
            }

            ConvertArguments = (ArgumentResult result, out object value) =>
            {
                if (convert(result, out var valueObj))
                {
                    value = valueObj;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            };

            if (getDefaultValue != default)
            {
                SetDefaultValueFactory(() => getDefaultValue());
            }
        }

        public Argument(ParseArgument<T> parse, bool isDefault = false) : this()
        {
            if (parse == null)
            {
                throw new ArgumentNullException(nameof(parse));
            }

            if (isDefault)
            {
                SetDefaultValueFactory(argumentResult => parse(argumentResult));
            }

            ConvertArguments = (ArgumentResult argumentResult, out object value) =>
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
    }
}
