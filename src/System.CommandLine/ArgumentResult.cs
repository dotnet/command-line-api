// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public abstract class ArgumentResult
    {
        internal ArgumentResult()
        {
        }

        public static FailedArgumentResult Failure(string error) => new FailedArgumentResult(error);

        public static SuccessfulArgumentResult<T> Success<T>(T value) => new SuccessfulArgumentResult<T>(value);
    }
}
