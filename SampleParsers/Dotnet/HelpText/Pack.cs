// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Pack
    {
        public const string HelpText =
            @".NET Core NuGet Package Packer

Usage: pack [arguments] [options] [args]

Arguments:
  <PROJECT>  The project to pack, defaults to the project file in the current directory. Can be a path to any project file

Options:
  -h|--help                           Show help information
  -o|--output <OUTPUT_DIR>            Directory in which to place built packages.
  --no-build                          Skip building the project prior to packing. By default, the project will be built.
  --include-symbols                   Include packages with symbols in addition to regular packages in output directory.
  --include-source                    Include PDBs and source files. Source files go into the src folder in the resulting nuget package
  -c|--configuration <CONFIGURATION>  Configuration to use for building the project.  Default for most projects is  ""Debug"".
  --version-suffix <VERSION_SUFFIX>   Defines the value for the $(VersionSuffix) property in the project.
  -s|--serviceable                    Set the serviceable flag in the package. For more information, please see https://aka.ms/nupkgservicing.
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.
";
    }
}