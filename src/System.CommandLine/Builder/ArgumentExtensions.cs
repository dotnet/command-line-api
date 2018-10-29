// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public static class ArgumentExtensions
    {
        public static Argument FromAmong(
            this Argument argument,
            params string[] values)
        {
            argument.AddValidValues(values);
            argument.AddSuggestions(values);

            return argument;
        }

        public static Argument WithDefaultValue(
            this Argument argument,
            Func<object> defaultValue)
        {
            argument.SetDefaultValue(defaultValue);

            return argument;
        }
    }
}
