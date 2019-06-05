// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal abstract class ArgumentResult
    {
        private protected ArgumentResult(IArgument argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        public IArgument Argument { get; }

        internal string ErrorMessage { get; set; }

        internal static FailedArgumentResult Failure(IArgument argument, string error) => new FailedArgumentResult(argument, error);

        public static SuccessfulArgumentResult Success(IArgument argument, object value) => new SuccessfulArgumentResult(argument, value);

        internal static NoArgumentResult None(IArgument argument) => new NoArgumentResult(argument);
    }
}
