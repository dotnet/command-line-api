// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public abstract class ArgumentResult
    {
        private protected ArgumentResult()
        {
        }

        public static FailedArgumentResult Failure(string error) => new FailedArgumentResult(error);

        public static SuccessfulArgumentResult Success(object value) => new SuccessfulArgumentResult(value);

        public static NoArgumentResult None { get; } = new NoArgumentResult();
    }
}
