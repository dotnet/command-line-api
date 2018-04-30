// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class SuccessfulArgumentParseResult<T> : ArgumentParseResult
    {
        public SuccessfulArgumentParseResult(T value = default(T))
        {
            Value = value;
        }

        public T Value { get; }

        public override bool Successful { get; } = true;
    }
}