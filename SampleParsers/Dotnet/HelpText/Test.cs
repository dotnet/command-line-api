// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Test
    {
        public const string HelpText =
            @".NET Test Driver

Usage: dotnet test [arguments] [options] [args]

Arguments:
  <PROJECT>  The project to test, defaults to the current directory.

Options:
  -h|--help                             Show help information
  -s|--settings <SETTINGS_FILE>         Settings to use when running tests.
  -t|--list-tests                       Lists discovered tests
  --filter <EXPRESSION>                 Run tests that match the given expression.
                                        Examples:
                                        Run tests with priority set to 1: --filter ""Priority = 1""
                                        Run a test with the specified full name: --filter ""FullyQualifiedName=Namespace.ClassName.MethodName""
                                        Run tests that contain the specified name: --filter ""FullyQualifiedName~Namespace.Class""
                                        More info on filtering support: https://aka.ms/vstest-filtering

  -a|--test-adapter-path                Use custom adapters from the given path in the test run.
                                        Example: --test-adapter-path <PATH_TO_ADAPTER>
  -l|--logger <LoggerUri/FriendlyName>  Specify a logger for test results.
                                        Example: --logger ""trx[;LogFileName=<Defaults to unique file name>]""
  -c|--configuration <CONFIGURATION>    Configuration to use for building the project.  Default for most projects is  ""Debug"".
  -f|--framework <FRAMEWORK>            Looks for test binaries for a specific framework
  -o|--output <OUTPUT_DIR>              Directory in which to find the binaries to be run
  -d|--diag <PATH_TO_FILE>              Enable verbose logs for test platform.
                                        Logs are written to the provided file.
  --no-build                            Do not build project before testing.
  -v|--verbosity                        Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra command-line runsettings arguments that should be passed to vstest. See 'dotnet vstest --help' for available options.
                                        Example: -- RunConfiguration.ResultsDirectory=""C:\users\user\desktop\Results Directory"" MSTest.DeploymentEnabled=false
 ";
    }
}