// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class Add
    {
        public static class Package
        {
            public const string HelpText =
                @".NET Add Package reference Command

Usage: dotnet add <PROJECT> package [arguments] [options]

Arguments:
  <PACKAGE_NAME>  Package references to add
  <PROJECT>       The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help                                Show help information
  -v|--version <VERSION>                   Version for the package to be added.
  -f|--framework <FRAMEWORK>               Add reference only when targetting a specific framework
  -n|--no-restore                          Add reference without performing restore preview and compatibility check.
  -s|--source <SOURCE>                     Use specific NuGet package sources to use during the restore.
  --package-directory <PACKAGE_DIRECTORY>  Restore the packages to this Directory .
  ";
        }
    }
}