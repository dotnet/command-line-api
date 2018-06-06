// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class SuccessfulArgumentParseResult<T> : SuccessfulArgumentParseResult
    {
        public SuccessfulArgumentParseResult(T value)
        {
            Value = value;
        }

        internal override bool HasValue => true;

        public T Value { get; }
    }
}
