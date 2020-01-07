﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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

        public Argument(string name, Func<T> defaultValue) : this(name)
        {
            SetDefaultValue(() => defaultValue());
        }

        public Argument(Func<T> defaultValue) : this()
        {
            SetDefaultValue(() => defaultValue());
        }

        public Argument(TryConvertArgument<T> convert, Func<T> defaultValue = default) : this()
        {
            if (convert == null)
            {
                throw new ArgumentNullException(nameof(convert));
            }

            ConvertArguments = (SymbolResult result, out object value) =>
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

            if (defaultValue != default)
            {
                SetDefaultValue(() => defaultValue());
            }
        }
    }
}
