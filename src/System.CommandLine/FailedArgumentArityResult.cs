// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal class FailedArgumentArityResult : FailedArgumentParseResult
    {
        public FailedArgumentArityResult(string errorMessage) : base(errorMessage)
        {
        }
    }
}
