// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class Argument<T> : Argument
    {
        public Argument()
        {
            ArgumentType = typeof(T);
        }

        public Argument(T defaultValue) : this()
        {
            SetDefaultValue(defaultValue);
        }

        public Argument(Func<T> defaultValue) : this()
        {
            SetDefaultValue(() => defaultValue());
        }

        public Argument(ConvertArgument convert) : this()
        {
            ConvertArguments = convert ?? throw new ArgumentNullException(nameof(convert));
        }
    }
}
