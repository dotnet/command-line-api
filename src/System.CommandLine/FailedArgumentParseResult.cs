// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace System.CommandLine
{
    public class FailedArgumentParseResult : ArgumentParseResult
    {
        public FailedArgumentParseResult(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(errorMessage));
            }

            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }
}
