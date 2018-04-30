// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class FailedArgumentParseResult : ArgumentParseResult
    {
        public string Error { get; }

        public FailedArgumentParseResult(string error)
        {
            Error = error;
        }

        public override bool Successful { get; } = false;
    }
}