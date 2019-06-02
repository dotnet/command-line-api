// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public abstract class ArgumentResult
    {
        private protected ArgumentResult(IArgument argument)
        {
            if (argument == null)
            {
                 throw new ArgumentNullException(nameof(argument));
            }

            Argument = argument;
        }

        public IArgument Argument { get; }

        internal string ErrorMessage { get; set; }

        public static FailedArgumentResult Failure(IArgument argument, string error) => new FailedArgumentResult(argument, error);

        public static SuccessfulArgumentResult Success(IArgument argument, object value) => new SuccessfulArgumentResult(argument, value);

        internal static NoArgumentResult None(IArgument argument) => new NoArgumentResult(argument);
    }
}
