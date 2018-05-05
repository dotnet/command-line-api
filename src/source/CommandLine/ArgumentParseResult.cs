// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public abstract class ArgumentParseResult
    {
        public abstract bool IsSuccessful { get; }

        public static FailedArgumentParseResult Failure(string error) => new FailedArgumentParseResult(error);

        public static SuccessfulArgumentParseResult<T> Success<T>(T value) => new SuccessfulArgumentParseResult<T>(value);
    }
}