// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public abstract class ArgumentParseResult
    {
        internal ArgumentParseResult()
        {
        }

        public static FailedArgumentParseResult Failure(string error) => new FailedArgumentParseResult(error);

        public static SuccessfulArgumentParseResult<T> Success<T>(T value) => new SuccessfulArgumentParseResult<T>(value);
    }
}
