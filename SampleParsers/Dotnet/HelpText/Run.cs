// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Run
    {
        public const string HelpText =
            @".NET Run Command

Usage: dotnet run [options] [[--] <additional arguments>...]]

Options:
  -h|--help                   Show help information
  -c|--configuration          Configuration to use for building the project. Default for most projects is ""Debug"".
  -f|--framework <FRAMEWORK>  Build and run the app using the specified framework. The framework has to be specified in the project file.
  -p|--project                The path to the project file to run (defaults to the current directory if there is only one project).

Additional Arguments:
 Arguments passed to the application that is being run.";
    }
}