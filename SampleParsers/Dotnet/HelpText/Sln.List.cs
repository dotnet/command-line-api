// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public partial class Sln
    {
        public static class List
        {
            public const string HelpText = @".NET List project(s) in a solution file Command

Usage: dotnet sln <SLN_FILE> list [options]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information";
        }
    }
}