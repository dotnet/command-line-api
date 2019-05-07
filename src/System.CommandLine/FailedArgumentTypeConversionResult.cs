// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal class FailedArgumentTypeConversionResult : FailedArgumentResult
    {
        // TODO: (FailedArgumentTypeConversionResult) localize
        internal FailedArgumentTypeConversionResult(
            IArgument argument,
            Type type,
            string value) :
            base(argument, $"Cannot parse argument '{value}' as {type}.")
        {
        }
    }
}
