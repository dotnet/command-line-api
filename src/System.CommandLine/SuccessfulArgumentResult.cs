// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class SuccessfulArgumentResult : ArgumentResult
    {
        protected SuccessfulArgumentResult()
        {
        }

        internal virtual bool HasValue => false;

        internal static ArgumentResult Empty { get; } = new SuccessfulArgumentResult();
    }
}
