// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class Add
    {
        public const string HelpText =
            @".NET Add Command

Usage: dotnet add [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  package    Command to add package reference
  reference  Command to add project to project reference

Use ""dotnet add [command] --help"" for more information about a command.";
    }
}