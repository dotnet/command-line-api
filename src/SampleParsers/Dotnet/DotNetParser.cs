// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public static class DotNetParser
    {
        public static CommandParser Instance = new CommandParser(
            commands: DotnetCommand());

        private static Command DotnetCommand() =>
            Command("dotnet",
                    ".NET Command Line Tools (2.0.0-alpha-alpha-004866)",
                    Define.Arguments().ZeroOrMore(),
                    New(),
                    Restore(),
                    Build(),
                    Publish(),
                    Run(),
                    Test(),
                    Pack(),
                    Migrate(),
                    Clean(),
                    Sln(),
                    Add(),
                    Remove(),
                    List(),
                    NuGet(),
                    Command("msbuild", ""),
                    Command("vstest", ""),
                    Complete(),
                    HelpOption(),
                    VerbosityOption());

        private static Command Complete() =>
            Command("complete", "",
                Define.Arguments().WithHelp(name: "path")
                    .ExactlyOne(),
                    Option("--position", "",
                        Define.Arguments().WithHelp(name: "command")
                            .ExactlyOne()));

        private static Command Add() =>
            Command("add",
                    ".NET Add Command",
                Define.Arguments().WithHelp(name: "PROJECT",
                    description: "The project file to operate on. If a file is not specified, the command will search the current directory for one.")
                    .ExactlyOne(),
                    Package(),
                    Reference(),
                    HelpOption());

        private static Command Build() =>
            Command("build",
                    ".NET Builder",
                Define.Arguments().WithHelp(name: "PROJECT",
                    description:
                    "The MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.")
                    .ZeroOrOne(),
                    HelpOption(),
                    Option("-o|--output",
                           "Output directory in which to place built artifacts.",
                        Define.Arguments().WithHelp(name: "OUTPUT_DIR")
                            .ExactlyOne()),
                    Option("-f|--framework",
                           "Target framework to build for. The target framework has to be specified in the project file.",
                           Define.Arguments().FromAmong(FrameworksFromProjectFile()).ExactlyOne()),
                    Option("-r|--runtime",
                           "Target runtime to build for. The default is to build a portable application.",
                           Define.Arguments().FromAmong(RunTimesFromProjectFile()).ExactlyOne()),
                    Option("-c|--configuration",
                           "Configuration to use for building the project. Default for most projects is  \"Debug\".",
                        Define.Arguments().WithHelp(name: "CONFIGURATION")
                            .WithDefaultValue(() => "DEBUG")
                            .ExactlyOne()),
                    Option("--version-suffix", "Defines the value for the $(VersionSuffix) property in the project",
                            Define.Arguments().WithHelp(name: "VERSION_SUFFIX")
                                .ExactlyOne()),
                    Option("--no-incremental", "Disables incremental build."),
                    Option("--no-dependencies", "Set this flag to ignore project-to-project references and only build the root project"),
                    VerbosityOption());

        private static Command Clean() =>
            Command("clean",
                    ".NET Clean Command",
                    HelpOption(),
                    Option("-o|--output", "Directory in which the build outputs have been placed.",
                        Define.Arguments().WithHelp(name: "OUTPUT_DIR")
                            .ExactlyOne()),
                    Option("-f|--framework", "Clean a specific framework.",
                        Define.Arguments().WithHelp(name: "FRAMEWORK")
                            .ExactlyOne()),
                    Option("-c|--configuration", "Clean a specific configuration.",
                        Define.Arguments().WithHelp(name: "CONFIGURATION")
                            .ExactlyOne()));

        private static Command List() =>
            Command("list",
                    ".NET List Command",
                Define.Arguments().WithHelp(name: "PROJECT",
                    description:
                    "The project file to operate on. If a file is not specified, the command will search the current directory for one.")
                    .ExactlyOne(),
                    HelpOption(),
                    Command("reference", "Command to list project to project references",
                        Define.Arguments()
                            .WithHelp(name: "PROJECT", description: "The project file to operate on. If a file is not specified, the command will search the current directory for one.")
                            .ExactlyOne(),
                            HelpOption()));

        private static Command Migrate() =>
            Command("migrate",
                    ".NET Migrate Command",
                    HelpOption(),
                    Option("-t|--template-file",
                           "Base MSBuild template to use for migrated app. The default is the project included in dotnet new."),
                    Option("-v|--sdk-package-version",
                           "The version of the SDK package that will be referenced in the migrated app. The default is the version of the SDK in dotnet new."),
                    Option("-x|--xproj-file",
                           "The path to the xproj file to use. Required when there is more than one xproj in a project directory."),
                    Option("-s|--skip-project-references",
                           "Skip migrating project references. By default, project references are migrated recursively."),
                    Option("-r|--report-file",
                           "Output migration report to the given file in addition to the console."),
                    Option("--format-report-file-json",
                           "Output migration report file as json rather than user messages."),
                    Option("--skip-backup",
                           "Skip moving project.json, global.json, and *.xproj to a `backup` directory after successful migration."));

        private static Command New() =>
            Command("new",
                    "Initialize .NET projects.",
                    Define.Arguments()
                          .AddSuggestions("console",
                                           "classlib",
                                           "mstest",
                                           "xunit",
                                           "web",
                                           "mvc",
                                           "webapi",
                                           "sln")
                          .ExactlyOne(),
                    Option("-l|--list",
                           "List templates containing the specified name."),
                    Option("-lang|--language",
                           "Specifies the language of the template to create",
                           Define.Arguments()
                                 .WithDefaultValue(() => "C#")
                                 .FromAmong("C#", "F#")
                                 .ExactlyOne()),
                    Option("-n|--name",
                           "The name for the output being created. If no name is specified, the name of the current directory is used."),
                    Option("-o|--output",
                           "Location to place the generated output."),
                    Option("-h|--help",
                           "Displays help for this command."),
                    Option("-all|--show-all",
                           "Shows all templates"));

        private static Command NuGet() =>
            Command("nuget",
                    "NuGet Command Line 4.0.0.0",
                    HelpOption(),
                    Option("--version",
                           "Show version information"),
                    Option("-v|--verbosity",
                           "The verbosity of logging to use. Allowed values: Debug, Verbose, Information, Minimal, Warning, Error.",
                        Define.Arguments().WithHelp(name: "verbosity")
                            .ExactlyOne()),
                    Command("delete",
                            "Deletes a package from the server.",
                        Define.Arguments().WithHelp(name: "root",
                            description: "The Package Id and version.")
                            .ExactlyOne(),
                            HelpOption(),
                            Option("--force-english-output",
                                   "Forces the application to run using an invariant, English-based culture."),
                            Option("-s|--source",
                                   "Specifies the server URL",
                                Define.Arguments().WithHelp(name: "source")
                                    .ExactlyOne()),
                            Option("--non-interactive",
                                   "Do not prompt for user input or confirmations."),
                            Option("-k|--api-key",
                                   "The API key for the server.",
                                Define.Arguments().WithHelp(name: "apiKey")
                                    .ExactlyOne())),
                    Command("locals",
                            "Clears or lists local NuGet resources such as http requests cache, packages cache or machine-wide global packages folder.",
                        Define.Arguments().FromAmong(@"all", @"http-cache", @"global-packages", @"temp")
                            .ExactlyOne(),
                            HelpOption(),
                            Option("--force-english-output",
                                   "Forces the application to run using an invariant, English-based culture."),
                            Option("-c|--clear", "Clear the selected local resources or cache location(s)."),
                            Option("-l|--list", "List the selected local resources or cache location(s).")),
                    Command("push",
                            "Pushes a package to the server and publishes it.",
                            HelpOption(),
                            Option("--force-english-output",
                                   "Forces the application to run using an invariant, English-based culture."),
                            Option("-s|--source",
                                   "Specifies the server URL",
                                Define.Arguments().WithHelp(name: "source")
                                    .ExactlyOne()),
                            Option("-ss|--symbol-source",
                                   "Specifies the symbol server URL. If not specified, nuget.smbsrc.net is used when pushing to nuget.org.",
                                Define.Arguments().WithHelp(name: "source")
                                    .ExactlyOne()),
                            Option("-t|--timeout",
                                   "Specifies the timeout for pushing to a server in seconds. Defaults to 300 seconds (5 minutes).",
                                Define.Arguments().WithHelp(name: "timeout")
                                    .ExactlyOne()),
                            Option("-k|--api-key", "The API key for the server.",
                                Define.Arguments().WithHelp(name: "apiKey")
                                    .ExactlyOne()),
                            Option("-sk|--symbol-api-key", "The API key for the symbol server.",
                                Define.Arguments().WithHelp(name: "apiKey")
                                    .ExactlyOne()),
                            Option("-d|--disable-buffering",
                                   "Disable buffering when pushing to an HTTP(S) server to decrease memory usage."),
                            Option("-n|--no-symbols",
                                   "If a symbols package exists, it will not be pushed to a symbols server.")));

        private static Command Pack() =>
            Command("pack",
                    ".NET Core NuGet Package Packer",
                    HelpOption(),
                    Option("-o|--output",
                           "Directory in which to place built packages.",
                        Define.Arguments().WithHelp(name: "OUTPUT_DIR")
                            .ExactlyOne()),
                    Option("--no-build",
                           "Skip building the project prior to packing. By default, the project will be built."),
                    Option("--include-symbols",
                           "Include packages with symbols in addition to regular packages in output directory."),
                    Option("--include-source",
                           "Include PDBs and source files. Source files go into the src folder in the resulting nuget package"),
                    Option("-c|--configuration",
                           "Configuration to use for building the project.  Default for most projects is  \"Debug\".",
                        Define.Arguments().WithHelp(name: "CONFIGURATION").ExactlyOne()),
                    Option("--version-suffix",
                           "Defines the value for the $(VersionSuffix) property in the project.",
                        Define.Arguments().WithHelp(name: "VERSION_SUFFIX")
                            .ExactlyOne()),
                    Option("-s|--serviceable",
                           "Set the serviceable flag in the package. For more information, please see https://aka.ms/nupkgservicing."),
                    VerbosityOption()
            );

        private static Command Package() =>
            Command("package",
                    ".NET Add Package reference Command",
                    Define.Arguments()
                          .WithHelp(name: "PACKAGE_NAME",
                                                description: "Package references to add")
                          .AddSuggestionSource(QueryNuGet)
                          .ExactlyOne(),
                    HelpOption(),
                    Option("-v|--version",
                           "Version for the package to be added.",
                           Define.Arguments().WithHelp(name: "VERSION")
                                 .ExactlyOne()),
                    Option("-f|--framework",
                           "Add reference only when targetting a specific framework",
                           Define.Arguments().WithHelp(name: "FRAMEWORK")
                                 .ExactlyOne()),
                    Option("-n|--no-restore ",
                           "Add reference without performing restore preview and compatibility check."),
                    Option("-s|--source",
                           "Use specific NuGet package sources to use during the restore."),
                    Option("--package-directory",
                           "Restore the packages to this Directory .",
                           Define.Arguments().WithHelp(name: "PACKAGE_DIRECTORY")
                                 .ExactlyOne()));

        private static Command Publish() =>
            Command("publish",
                    ".NET Publisher",
                    Define.Arguments().ExactlyOne(),
                    HelpOption(),
                    Option("-f|--framework",
                           "Target framework to publish for. The target framework has to be specified in the project file.",
                        Define.Arguments().WithHelp(name: "FRAMEWORK")
                            .ExactlyOne()),
                    Option("-r|--runtime",
                           "Publish the project for a given runtime.\nThis is used when creating self-contained deployment.\nDefault is to publish a framework-dependent app.",
                        Define.Arguments().WithHelp(name: "RUNTIME_IDENTIFIER")
                            .ExactlyOne()),
                    Option("-o|--output",
                           "Output directory in which to place the published artifacts.",
                        Define.Arguments().WithHelp(name: "OUTPUT_DIR")
                            .ExactlyOne()),
                    Option("-c|--configuration", "Configuration to use for building the project.  Default for most projects is  \"Debug\".",
                        Define.Arguments().WithHelp(name: "CONFIGURATION")
                            .ExactlyOne()),
                    Option("--version-suffix", "Defines the value for the $(VersionSuffix) property in the project.",
                        Define.Arguments().WithHelp(name: "VERSION_SUFFIX")
                            .ExactlyOne()),
                    VerbosityOption());

        private static Command Remove() =>
            Command("remove",
                    "",
                    HelpOption(),
                    Command("package",
                            "Command to remove package reference.",
                            HelpOption()),
                    Command("reference",
                            "Command to remove project to project reference",
                            HelpOption(),
                            Option("-f|--framework",
                                   "Remove reference only when targetting a specific framework",
                                Define.Arguments().WithHelp(name: "FRAMEWORK")
                                    .ExactlyOne())));

        private static Command Reference() =>
            Command("reference",
                    "Command to add project to project reference",
                    Define.Arguments().OneOrMore(),
                    HelpOption(),
                    Option("-f|--framework",
                           "Add reference only when targetting a specific framework",
                        Define.Arguments().WithHelp(name: "FRAMEWORK")
                            .ExactlyOne()));

        private static Command Restore() =>
            Command("restore",
                    ".NET dependency restorer",
                    HelpOption(),
                    Option("-s|--source",
                           "Specifies a NuGet package source to use during the restore.",
                        Define.Arguments().WithHelp(name: "SOURCE")
                            .ExactlyOne()),
                    Option("-r|--runtime",
                           "Target runtime to restore packages for.",
                        Define.Arguments().FromAmong(RunTimesFromProjectFile())
                            .WithHelp(name: "RUNTIME_IDENTIFIER")
                            .ZeroOrOne()),
                    Option("--packages",
                           "Directory to install packages in.",
                        Define.Arguments().WithHelp(name: "PACKAGES_DIRECTORY")
                            .ExactlyOne()),
                    Option("--disable-parallel",
                           "Disables restoring multiple projects in parallel."),
                    Option("--configfile",
                           "The NuGet configuration file to use.",
                        Define.Arguments().WithHelp(name: "FILE")
                            .ExactlyOne()),
                    Option("--no-cache",
                           "Do not cache packages and http requests."),
                    Option("--ignore-failed-sources",
                           "Treat package source failures as warnings."),
                    Option("--no-dependencies",
                           "Set this flag to ignore project to project references and only restore the root project"),
                    VerbosityOption());

        private static Command Run()
        {
            var option = Option("-p|--project",
                                "The path to the project file to run (defaults to the current directory if there is only one project).",
                                Define.Arguments().ZeroOrOne());
            var option1 = Option("-f|--framework",
                                 "Build and run the app using the specified framework. The framework has to be specified in the project file.",
                                 Define.Arguments().FromAmong(FrameworksFromProjectFile()).ExactlyOne());

            var option2 = Option("-c|--configuration",
                                 @"Configuration to use for building the project. Default for most projects is ""Debug"".");

            return Command("run",
                           ".NET Run Command",
                           treatUnmatchedTokensAsErrors: false,
                           arguments: ArgumentsRule.None,
                           options: new[]
                           {
                               HelpOption(),
                               option2,
                               option1,
                               option
                           });
        }

        private static Command Sln() =>
            Command("sln",
                    ".NET modify solution file command",
                Define.Arguments().WithHelp(name: "SLN_FILE",
                    description: "Solution file to operate on. If not specified, the command will search the current directory for one.")
                    .WithDefaultValue(Directory.GetCurrentDirectory)
                    .ExistingFilesOnly()
                    .ExactlyOne(),
                    HelpOption(),
                    Command("add",
                            ".NET Add project(s) to a solution file Command",
                        Define.Arguments().WithHelp(name: "args",
                            description: "Add one or more specified projects to the solution.")
                            .OneOrMore(),
                            HelpOption()),
                    Command("list",
                            "List all projects in the solution.",
                            HelpOption()),
                    Command("remove",
                            "Remove the specified project(s) from the solution. The project is not impacted.",
                        Define.Arguments().WithHelp(name: "args",
                            description: "Remove the specified project(s) from the solution. The project is not impacted.")
                            .OneOrMore(),
                            HelpOption()));

        private static Command Test() =>
            Command("test",
                    ".NET Test Driver",
                Define.Arguments().WithHelp(name: "PROJECT",
                    description: "The project to test, defaults to the current directory.")
                    .ExactlyOne(),
                    Option("-h|--help",
                           "Show help information"),
                    Option("-s|--settings",
                           "Settings to use when running tests.",
                        Define.Arguments().WithHelp(name: "SETTINGS_FILE")
                            .ExactlyOne()),
                    Option("-t|--list-tests",
                           "Lists discovered tests"),
                    Option("--filter",
                           @"Run tests that match the given expression.
                             Examples:
                             Run tests with priority set to 1: --filter ""Priority = 1""
                             Run a test with the specified full name: --filter ""FullyQualifiedName=Namespace.ClassName.MethodName""
                             Run tests that contain the specified name: --filter ""FullyQualifiedName~Namespace.Class""
                             More info on filtering support: https://aka.ms/vstest-filtering",
                        Define.Arguments().WithHelp(name: "EXPRESSION")
                            .ExactlyOne()),
                    Option("-a|--test-adapter-path",
                           "Use custom adapters from the given path in the test run.\r\n                          Example: --test-adapter-path <PATH_TO_ADAPTER>"),
                    Option("-l|--logger",
                           "Specify a logger for test results.\r\n                          Example: --logger \"trx[;LogFileName=<Defaults to unique file name>]\"",
                        Define.Arguments().WithHelp(name: "LoggerUri/FriendlyName")
                            .ExactlyOne()),
                    Option("-c|--configuration", "Configuration to use for building the project.  Default for most projects is  \"Debug\".",
                        Define.Arguments().WithHelp(name: "CONFIGURATION")
                            .ExactlyOne()),
                    Option("-f|--framework",
                           "Looks for test binaries for a specific framework",
                        Define.Arguments().WithHelp(name: "FRAMEWORK")
                            .ExactlyOne()),
                    Option("-o|--output",
                           "Directory in which to find the binaries to be run",
                        Define.Arguments().WithHelp(name: "OUTPUT_DIR")
                            .ExactlyOne()),
                    Option("-d|--diag",
                           "Enable verbose logs for test platform.\r\n                          Logs are written to the provided file.",
                        Define.Arguments().WithHelp(name: "PATH_TO_FILE")
                            .ExactlyOne()),
                    Option("--no-build",
                           "Do not build project before testing."),
                    VerbosityOption());

        private static Option HelpOption() =>
            Option("-h|--help",
                   "Show help information",
                   ArgumentsRule.None);

        private static Option VerbosityOption() =>
            Option("-v|--verbosity",
                   "Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]",
                   Define.Arguments()
                         .FromAmong("q[uiet]",
                                    "m[inimal]",
                                    "n[ormal]",
                                    "d[etailed]")
                         .ExactlyOne());

        private static string[] FrameworksFromProjectFile() =>
            new string[] { };

        public static string[] KnownRuntimes =
        {
            "win10-x86",
            "win10-x64",
            "win10-arm64",
            "osx.10.11-x64",
            "centos.7-x64",
            "debian.8-x64",
            "linuxmint.17-x64",
            "opensuse.13.2-x64",
            "rhel.7.2-x64",
            "ubuntu.14.04-x64",
            "ubuntu.16.04-x64",
        };

        private static IEnumerable<string> QueryNuGet(
            ParseResult parseResult,
            int? position = null)
        {
            var httpClient = new HttpClient();

            string result;

            string textToMatch = parseResult.TextToMatch();

            try
            {
                var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = httpClient.GetAsync($"https://api-v2v3search-0.nuget.org/query?q={textToMatch}&skip=0&take=100&prerelease=true", cancellation.Token)
                                         .Result;

                result = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                yield break;
            }

            var json = JObject.Parse(result);

            foreach (var id in json["data"])
            {
                yield return id["id"].Value<string>();
            }
        }

        private static string[] RunTimesFromProjectFile() => Array.Empty<string>();
    }
}