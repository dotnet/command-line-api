// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Build
    {
        public const string HelpText = @".NET Builder

Usage: dotnet build [arguments] [options] [args]

Arguments:
  <PROJECT>  The MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.

Options:
  -h|--help                           Show help information
  -o|--output <OUTPUT_DIR>            Output directory in which to place built artifacts.
  -f|--framework <FRAMEWORK>          Target framework to build for. The target framework has to be specified in the project file.
  -r|--runtime <RUNTIME_IDENTIFIER>   Target runtime to build for. The default is to build a portable application.
  -c|--configuration <CONFIGURATION>  Configuration to use for building the project. Default for most projects is  ""Debug"".
  --version-suffix <VERSION_SUFFIX>   Defines the value for the $(VersionSuffix) property in the project
  --no-incremental                    Disables incremental build.
  --no-dependencies                   Set this flag to ignore project-to-project references and only build the root project
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.";
    }
}