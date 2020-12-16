// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class SuccessfulArgumentConversionResult : ArgumentConversionResult
    {
        internal SuccessfulArgumentConversionResult(
            IArgument argument, 
            object? value) : base(argument)
        {
            Value = value;
        }
        
        internal SuccessfulArgumentConversionResult(
            IArgument argument, 
            int skippedTokensFromEnd,
            object? value) : base(argument)
        {
            SkippedTokensFromEnd = skippedTokensFromEnd;
            Value = value;
        }

        public int SkippedTokensFromEnd { get; }

        public object? Value { get; }
    }
}
