// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal abstract class ArgumentConversionResult
    {
        private protected ArgumentConversionResult(IArgument argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        public IArgument Argument { get; }

        internal string? ErrorMessage { get; set; }

        internal static FailedArgumentConversionResult Failure(IArgument argument, string error) => new(argument, error);

        public static SuccessfulArgumentConversionResult Success(IArgument argument, object? value) => new(argument, value);

        internal static NoArgumentConversionResult None(IArgument argument) => new(argument);
    }
}