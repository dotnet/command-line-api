// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class HelpInvocationResult : IInvocationResult
    {
        public HelpInvocationResult(int returnCode, string standardOutput)
        {
            ReturnCode = returnCode;
            StandardOutput = standardOutput;
        }

        public int ReturnCode { get; }

        public string StandardOutput { get; }
    }
}
