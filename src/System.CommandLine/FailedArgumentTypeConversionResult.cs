// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal class FailedArgumentTypeConversionResult : FailedArgumentConversionResult
    {
        internal FailedArgumentTypeConversionResult(
            IArgument argument,
            Type type,
            string value) :
            base(argument, FormatErrorMessage(argument, type, value))
        {
        }

        private static string FormatErrorMessage(
            IArgument argument,
            Type type,
            string value)
        {
            if (argument is Argument a &&
                a.Parents.Count == 1)
            {
                // TODO: (FailedArgumentTypeConversionResult) localize

                var symbolType =
                    a.Parents[0] switch {
                        ICommand _ => "command",
                        IOption _ => "option",
                        _ => null
                        };

                var alias = a.Parents[0].RawAliases[0];

                return $"Cannot parse argument '{value}' for {symbolType} '{alias}' as expected type {type}.";
            }

            return $"Cannot parse argument '{value}' as expected type {type}.";
        }
    }
}
