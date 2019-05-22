// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public abstract class ArgumentResult
    {
        private protected ArgumentResult(IArgument argument)
        {
            Argument = argument;
        }

        public IArgument Argument { get; }

        internal string ErrorMessage { get; set; }

        public static FailedArgumentResult Failure(Argument argument, string error) => new FailedArgumentResult(argument, error);

        public static SuccessfulArgumentResult Success(IArgument argument, object value) => new SuccessfulArgumentResult(argument, value);

        public static NoArgumentResult None(IArgument argument = null) => new NoArgumentResult(argument);
    }
}
