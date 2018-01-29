using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public class DotnetParserHelpViewTests
    {
        private readonly ITestOutputHelper output;

        private readonly Parser dotnet = DotNetParser.Instance;

        public DotnetParserHelpViewTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory(Skip="Work in progress")]
        [InlineData("dotnet -h")]
        [InlineData("dotnet add -h")]
        [InlineData("dotnet add package -h")]
        [InlineData("dotnet add reference -h")]
        [InlineData("dotnet build -h")]
        [InlineData("dotnet clean -h")]
        [InlineData("dotnet list -h")]
        [InlineData("dotnet migrate -h")]
        //        [InlineData("dotnet msbuild -h")]
        [InlineData("dotnet new -h")]
        //        [InlineData("dotnet nuget -h")]
        [InlineData("dotnet pack -h")]
        [InlineData("dotnet publish -h")]
        [InlineData("dotnet remove -h")]
        [InlineData("dotnet restore -h")]
        [InlineData("dotnet run -h")]
        [InlineData("dotnet sln -h")]
        [InlineData("dotnet sln add -h")]
        [InlineData("dotnet sln list -h")]
        [InlineData("dotnet sln remove -h")]
        [InlineData("dotnet test -h")]
        //        [InlineData("dotnet vstest -h")]
        public void HelpText(string commandLine)
        {
            var result = dotnet.Parse(commandLine);

            var helpView = result.Command().HelpView();
            output.WriteLine(helpView);

            helpView.Should().MatchLineByLine(helpTextForCommandLine[commandLine]);
        }

        private static readonly Dictionary<string, string> helpTextForCommandLine = new Dictionary<string, string>
        {
            ["dotnet -h"] =
            @".NET Command Line Tools (2.0.0-alpha-alpha-004866)
Usage: dotnet [host-options] [command] [arguments] [common-options]

Arguments:
  [command]             The command to execute
  [arguments]           Arguments to pass to the command
  [host-options]        Options specific to dotnet (host)
  [common-options]      Options common to all commands

Common options:
  -v|--verbose          Enable verbose output
  -h|--help             Show help

Host options (passed before the command):
  -d|--diagnostics      Enable diagnostic output
  --version             Display .NET CLI Version Number
  --info                Display .NET CLI Info

Commands:
  new           Initialize .NET projects.
  restore       Restore dependencies specified in the .NET project.
  build         Builds a .NET project.
  publish       Publishes a .NET project for deployment (including the runtime).
  run           Compiles and immediately executes a .NET project.
  test          Runs unit tests using the test runner specified in the project.
  pack          Creates a NuGet package.
  migrate       Migrates a project.json based project to a msbuild based project.
  clean         Clean build output(s).
  sln           Modify solution (SLN) files.

Project modification commands:
  add           Add items to the project
  remove        Remove items from the project
  list          List items in the project

Advanced Commands:
  nuget         Provides additional NuGet commands.
  msbuild       Runs Microsoft Build Engine (MSBuild).
  vstest        Runs Microsoft Test Execution Command Line Tool.
",
            ["dotnet add -h"] =
            @".NET Add Command

Usage: dotnet add [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  package    Command to add package reference
  reference  Command to add project to project reference

Use ""dotnet add [command] --help"" for more information about a command.",

            ["dotnet add package -h"] =
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
  ",

            ["dotnet add reference -h"] =
            @".NET Add Project to Project reference Command

Usage: dotnet add <PROJECT> reference [options] [args]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help                   Show help information
  -f|--framework <FRAMEWORK>  Add reference only when targetting a specific framework

Additional Arguments:
 Project to project references to add
  ",
            ["dotnet build -h"] =
            @".NET Builder

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
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.",

            ["dotnet clean -h"] =
            @".NET Clean Command

Usage: dotnet clean [arguments] [options] [args]

Arguments:
  <PROJECT>  The MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.

Options:
  -h|--help                           Show help information
  -o|--output <OUTPUT_DIR>            Directory in which the build outputs have been placed.
  -f|--framework <FRAMEWORK>          Clean a specific framework.
  -c|--configuration <CONFIGURATION>  Clean a specific configuration.
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.
",

            ["dotnet list -h"] =
            @".NET List Command

Usage: dotnet list [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  reference  Command to list project to project references

Use ""dotnet list [command] --help"" for more information about a command.",

            ["dotnet list reference -h"] =
            @".NET Core Project-to-Project dependency viewer

Usage: dotnet list <PROJECT> reference [options]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information",

            ["dotnet migrate -h"] =
            @".NET Migrate Command

Usage: dotnet migrate [arguments] [options]

Arguments:
  <PROJECT_JSON/GLOBAL_JSON/SOLUTION_FILE/PROJECT_DIR>  The path to one of the following:
    - a project.json file to migrate.
    - a global.json file, it will migrate the folders specified in global.json.
    - a solution.sln file, it will migrate the projects referenced in the solution.
    - a directory to migrate, it will recursively search for project.json files to migrate.
Defaults to current directory if nothing is specified.

Options:
  -h|--help                     Show help information
  -t|--template-file            Base MSBuild template to use for migrated app. The default is the project included in dotnet new.
  -v|--sdk-package-version      The version of the SDK package that will be referenced in the migrated app. The default is the version of the SDK in dotnet new.
  -x|--xproj-file               The path to the xproj file to use. Required when there is more than one xproj in a project directory.
  -s|--skip-project-references  Skip migrating project references. By default, project references are migrated recursively.
  -r|--report-file              Output migration report to the given file in addition to the console.
  --format-report-file-json     Output migration report file as json rather than user messages.
  --skip-backup                 Skip moving project.json, global.json, and *.xproj to a `backup` directory after successful migration.
",

            ["dotnet msbuild -h"] =
            @"Microsoft (R) Build Engine version 15.2.2.21747

Syntax:              MSBuild.exe [options] [project file | directory]

Description:         Builds the specified targets in the project file. If
                     a project file is not specified, MSBuild searches the
                     current working directory for a file that has a file
                     extension that ends in ""proj"" and uses that file.  If
                     a directory is specified, MSBuild searches that
                     directory for a project file.

Switches:

  /target:<targets>  Build these targets in this project. Use a semicolon or a
                     comma to separate multiple targets, or specify each
                     target separately. (Short form: /t)
                     Example:
                       /target:Resources;Compile

  /property:<n>=<v>  Set or override these project-level properties. <n> is
                     the property name, and <v> is the property value. Use a
                     semicolon or a comma to separate multiple properties, or
                     specify each property separately. (Short form: /p)
                     Example:
                       /property:WarningLevel=2;OutDir=bin\Debug\

  /maxcpucount[:n]   Specifies the maximum number of concurrent processes to
                     build with. If the switch is not used, the default
                     value used is 1. If the switch is used without a value
                     MSBuild will use up to the number of processors on the
                     computer. (Short form: /m[:n])

  /toolsversion:<version>
                     The version of the MSBuild Toolset (tasks, targets, etc.)
                     to use during build. This version will override the
                     versions specified by individual projects. (Short form:
                     /tv)
                     Example:
                       /toolsversion:3.5

  /verbosity:<level> Display this amount of information in the event log.
                     The available verbosity levels are: q[uiet], m[inimal],
                     n[ormal], d[etailed], and diag[nostic]. (Short form: /v)
                     Example:
                       /verbosity:quiet

  /consoleloggerparameters:<parameters>
                     Parameters to console logger. (Short form: /clp)
                     The available parameters are:
                        PerformanceSummary--Show time spent in tasks, targets
                            and projects.
                        Summary--Show error and warning summary at the end.
                        NoSummary--Don't show error and warning summary at the
                            end.
                        ErrorsOnly--Show only errors.
                        WarningsOnly--Show only warnings.
                        NoItemAndPropertyList--Don't show list of items and
                            properties at the start of each project build.
                        ShowCommandLine--Show TaskCommandLineEvent messages
                        ShowTimestamp--Display the Timestamp as a prefix to any
                            message.
                        ShowEventId--Show eventId for started events, finished
                            events, and messages
                        ForceNoAlign--Does not align the text to the size of
                            the console buffer
                        DisableConsoleColor--Use the default console colors
                            for all logging messages.
                        DisableMPLogging-- Disable the multiprocessor
                            logging style of output when running in
                            non-multiprocessor mode.
                        EnableMPLogging--Enable the multiprocessor logging
                            style even when running in non-multiprocessor
                            mode. This logging style is on by default.
                        ForceConsoleColor--Use ANSI console colors even if
                            console does not support it
                        Verbosity--overrides the /verbosity setting for this
                            logger.
                     Example:
                        /consoleloggerparameters:PerformanceSummary;NoSummary;
                                                 Verbosity=minimal

  /noconsolelogger   Disable the default console logger and do not log events
                     to the console. (Short form: /noconlog)

  /fileLogger[n]     Logs the build output to a file. By default
                     the file is in the current directory and named
                     ""msbuild[n].log"". Events from all nodes are combined into
                     a single log. The location of the file and other
                     parameters for the fileLogger can be specified through
                     the addition of the ""/fileLoggerParameters[n]"" switch.
                     ""n"" if present can be a digit from 1-9, allowing up to
                     10 file loggers to be attached. (Short form: /fl[n])

  /fileloggerparameters[n]:<parameters>
                     Provides any extra parameters for file loggers.
                     The presence of this switch implies the
                     corresponding /filelogger[n] switch.
                     ""n"" if present can be a digit from 1-9.
                     /fileloggerparameters is also used by any distributed
                     file logger, see description of /distributedFileLogger.
                     (Short form: /flp[n])
                     The same parameters listed for the console logger are
                     available. Some additional available parameters are:
                        LogFile--path to the log file into which the
                            build log will be written.
                        Append--determines if the build log will be appended
                            to or overwrite the log file. Setting the
                            switch appends the build log to the log file;
                            Not setting the switch overwrites the
                            contents of an existing log file.
                            The default is not to append to the log file.
                        Encoding--specifies the encoding for the file,
                            for example, UTF-8, Unicode, or ASCII
                     Default verbosity is Detailed.
                     Examples:
                       /fileLoggerParameters:LogFile=MyLog.log;Append;
                                           Verbosity=diagnostic;Encoding=UTF-8

                       /flp:Summary;Verbosity=minimal;LogFile=msbuild.sum
                       /flp1:warningsonly;logfile=msbuild.wrn
                       /flp2:errorsonly;logfile=msbuild.err

  /distributedlogger:<central logger>*<forwarding logger>
                     Use this logger to log events from MSBuild, attaching a
                     different logger instance to each node. To specify
                     multiple loggers, specify each logger separately.
                     (Short form /dl)
                     The <logger> syntax is:
                       [<logger class>,]<logger assembly>[;<logger parameters>]
                     The <logger class> syntax is:
                       [<partial or full namespace>.]<logger class name>
                     The <logger assembly> syntax is:
                       {<assembly name>[,<strong name>] | <assembly file>}
                     The <logger parameters> are optional, and are passed
                     to the logger exactly as you typed them. (Short form: /l)
                     Examples:
                       /dl:XMLLogger,MyLogger,Version=1.0.2,Culture=neutral
                       /dl:MyLogger,C:\My.dll*ForwardingLogger,C:\Logger.dll

  /distributedFileLogger
                     Logs the build output to multiple log files, one log file
                     per MSBuild node. The initial location for these files is
                     the current directory. By default the files are called
                     ""MSBuild<nodeid>.log"". The location of the files and
                     other parameters for the fileLogger can be specified
                     with the addition of the ""/fileLoggerParameters"" switch.

                     If a log file name is set through the fileLoggerParameters
                     switch the distributed logger will use the fileName as a
                     template and append the node id to this fileName to
                     create a log file for each node.

  /logger:<logger>   Use this logger to log events from MSBuild. To specify
                     multiple loggers, specify each logger separately.
                     The <logger> syntax is:
                       [<logger class>,]<logger assembly>[;<logger parameters>]
                     The <logger class> syntax is:
                       [<partial or full namespace>.]<logger class name>
                     The <logger assembly> syntax is:
                       {<assembly name>[,<strong name>] | <assembly file>}
                     The <logger parameters> are optional, and are passed
                     to the logger exactly as you typed them. (Short form: /l)
                     Examples:
                       /logger:XMLLogger,MyLogger,Version=1.0.2,Culture=neutral
                       /logger:XMLLogger,C:\Loggers\MyLogger.dll;OutputAsHTML

  /warnaserror[:code[;code2]]
                     List of warning codes to treats as errors.  Use a semicolon
                     or a comma to separate multiple warning codes. To treat all
                     warnings as errors use the switch with no values.
                     (Short form: /err[:c;[c2]])

                     Example:
                       /warnaserror:MSB4130

                     When a warning is treated as an error the target will
                     continue to execute as if it was a warning but the overall
                     build will fail.

  /warnasmessage[:code[;code2]]
                     List of warning codes to treats as low importance
                     messages.  Use a semicolon or a comma to separate
                     multiple warning codes.
                     (Short form: /nowarn[:c;[c2]])

                     Example:
                       /warnasmessage:MSB3026

  /ignoreprojectextensions:<extensions>
                     List of extensions to ignore when determining which
                     project file to build. Use a semicolon or a comma
                     to separate multiple extensions.
                     (Short form: /ignore)
                     Example:
                       /ignoreprojectextensions:.sln

  /preprocess[:file]
                     Creates a single, aggregated project file by
                     inlining all the files that would be imported during a
                     build, with their boundaries marked. This can be
                     useful for figuring out what files are being imported
                     and from where, and what they will contribute to
                     the build. By default the output is written to
                     the console window. If the path to an output file
                     is provided that will be used instead.
                     (Short form: /pp)
                     Example:
                       /pp:out.txt

  /detailedsummary
                     Shows detailed information at the end of the build
                     about the configurations built and how they were
                     scheduled to nodes.
                     (Short form: /ds)

  @<file>            Insert command-line settings from a text file. To specify
                     multiple response files, specify each response file
                     separately.

                     Any response files named ""msbuild.rsp"" are automatically
                     consumed from the following locations:
                     (1) the directory of msbuild.exe
                     (2) the directory of the first project or solution built

  /noautoresponse    Do not auto-include any MSBuild.rsp files. (Short form:
                     /noautorsp)

  /nologo            Do not display the startup banner and copyright message.

  /version           Display version information only. (Short form: /ver)

  /help              Display this usage message. (Short form: /? or /h)

Examples:

        MSBuild MyApp.sln /t:Rebuild /p:Configuration=Release
        MSBuild MyApp.csproj /t:Clean
                             /p:Configuration=Debug;TargetFrameworkVersion=v3.5",

            ["dotnet new -h"] =
            @"Usage: dotnet new [arguments] [options]

Arguments:
  template  The template to instantiate.

Options:
  -l|--list         List templates containing the specified name.
  -lang|--language  Specifies the language of the template to create
  -n|--name         The name for the output being created. If no name is specified, the name of the current directory is used.
  -o|--output       Location to place the generated output.
  -h|--help         Displays help for this command.
  -all|--show-all   Shows all templates


Templates                                 Short Name      Language      Tags
--------------------------------------------------------------------------------------
Console Application                       console         [C#], F#      Common/Console
Class library                             classlib        [C#], F#      Common/Library
Unit Test Project                         mstest          [C#], F#      Test/MSTest
xUnit Test Project                        xunit           [C#], F#      Test/xUnit
Empty ASP.NET Core Web Application        web             [C#]          Web/Empty
MVC ASP.NET Core Web Application          mvc             [C#], F#      Web/MVC
Web API ASP.NET Core Web Application      webapi          [C#]          Web/WebAPI
Solution File                             sln                           Solution

Examples:
    dotnet new mvc --auth None --framework netcoreapp1.0
    dotnet new mvc --framework netcoreapp1.0
    dotnet new --help",

            ["dotnet nuget -h"] =
            @"NuGet Command Line 4.0.0.0

Usage: dotnet nuget [options] [command]

Options:
  -h|--help                   Show help information
  --version                   Show version information
  -v|--verbosity <verbosity>  The verbosity of logging to use. Allowed values: Debug, Verbose, Information, Minimal, Warning, Error.

Commands:
  delete  Deletes a package from the server.
  locals  Clears or lists local NuGet resources such as http requests cache, packages cache or machine-wide global packages folder.
  push    Pushes a package to the server and publishes it.

Use ""dotnet nuget [command] --help"" for more information about a command.",

            ["dotnet nuget delete -h"] =
            @"Usage: dotnet nuget delete [arguments] [options]

Arguments:
  [root]  The Package Id and version.

Options:
  -h|--help               Show help information
  --force-english-output  Forces the application to run using an invariant, English-based culture.
  -s|--source <source>    Specifies the server URL
  --non-interactive       Do not prompt for user input or confirmations.
  -k|--api-key <apiKey>   The API key for the server.",

            ["dotnet nuget locals -h"] =
            @"Usage: dotnet nuget locals [arguments] [options]

Arguments:
  Cache Location(s)  Specifies the cache location(s) to list or clear.
<all | http-cache | global-packages | temp>

Options:
  -h|--help               Show help information
  --force-english-output  Forces the application to run using an invariant, English-based culture.
  -c|--clear              Clear the selected local resources or cache location(s).
  -l|--list               List the selected local resources or cache location(s).",

            ["dotnet nuget push -h"] =
            @"Usage: dotnet nuget push [arguments] [options]

Arguments:
  [root]  Specify the path to the package and your API key to push the package to the server.

Options:
  -h|--help                      Show help information
  --force-english-output         Forces the application to run using an invariant, English-based culture.
  -s|--source <source>           Specifies the server URL
  -ss|--symbol-source <source>   Specifies the symbol server URL. If not specified, nuget.smbsrc.net is used when pushing to nuget.org.
  -t|--timeout <timeout>         Specifies the timeout for pushing to a server in seconds. Defaults to 300 seconds (5 minutes).
  -k|--api-key <apiKey>          The API key for the server.
  -sk|--symbol-api-key <apiKey>  The API key for the symbol server.
  -d|--disable-buffering         Disable buffering when pushing to an HTTP(S) server to decrease memory usage.
  -n|--no-symbols                If a symbols package exists, it will not be pushed to a symbols server.",

            ["dotnet pack -h"] =
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
",

            ["dotnet publish -h"] =
            @".NET Publisher

Usage: dotnet publish [arguments] [options] [args]

Arguments:
  <PROJECT>  The MSBuild project file to publish. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.

Options:
  -h|--help                           Show help information
  -f|--framework <FRAMEWORK>          Target framework to publish for. The target framework has to be specified in the project file.
  -r|--runtime <RUNTIME_IDENTIFIER>   Publish the project for a given runtime. This is used when creating self-contained deployment. Default is to publish a framework-dependent app.
  -o|--output <OUTPUT_DIR>            Output directory in which to place the published artifacts.
  -c|--configuration <CONFIGURATION>  Configuration to use for building the project.  Default for most projects is  ""Debug"".
  --version-suffix <VERSION_SUFFIX>   Defines the value for the $(VersionSuffix) property in the project.
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.",

            ["dotnet remove -h"] =
            @".NET Remove Command

Usage: dotnet remove [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  package    Command to remove package reference.
  reference  Command to remove project to project reference

Use ""dotnet remove [command] --help"" for more information about a command.
",

            ["dotnet remove package -h"] =
            @".NET Remove Package reference Command.

Usage: dotnet remove <PROJECT> package [options] [args]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Additional Arguments:
 Package reference to remove.
   ",

            ["dotnet remove reference -h"] =
            @".NET Remove Project to Project reference Command

Usage: dotnet remove <PROJECT> reference [options] [args]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help                   Show help information
  -f|--framework <FRAMEWORK>  Remove reference only when targetting a specific framework

Additional Arguments:
 Project to project references to remove",

            ["dotnet restore -h"] =
            @".NET dependency restorer

Usage: restore [arguments] [options] [args]

Arguments:
  [PROJECT]  Optional path to a project file or MSBuild arguments.

Options:
  -h|--help                          Show help information
  -s|--source <SOURCE>               Specifies a NuGet package source to use during the restore.
  -r|--runtime <RUNTIME_IDENTIFIER>  Target runtime to restore packages for.
  --packages <PACKAGES_DIRECTORY>    Directory to install packages in.
  --disable-parallel                 Disables restoring multiple projects in parallel.
  --configfile <FILE>                The NuGet configuration file to use.
  --no-cache                         Do not cache packages and http requests.
  --ignore-failed-sources            Treat package source failures as warnings.
  --no-dependencies                  Set this flag to ignore project to project references and only restore the root project
  -v|--verbosity                     Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.",

            ["dotnet run -h"] =
            @".NET Run Command

Usage: dotnet run [options] [[--] <additional arguments>...]]

Options:
  -h|--help                   Show help information
  -c|--configuration          Configuration to use for building the project. Default for most projects is ""Debug"".
  -f|--framework <FRAMEWORK>  Build and run the app using the specified framework. The framework has to be specified in the project file.
  -p|--project                The path to the project file to run (defaults to the current directory if there is only one project).

Additional Arguments:
 Arguments passed to the application that is being run.",

            ["dotnet sln add -h"] =
            @".NET Add project(s) to a solution file Command

Usage: dotnet sln <SLN_FILE> add [options] [args]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Additional Arguments:
 Add a specified project(s) to the solution.",

            ["dotnet sln -h"] =
            @".NET modify solution file command

Usage: dotnet sln [arguments] [options] [command]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  add     Add a specified project(s) to the solution.
  list    List all projects in the solution.
  remove  Remove the specified project(s) from the solution. The project is not impacted.

Use ""dotnet sln [command] --help"" for more information about a command.
",

            ["dotnet sln list -h"] =
            @".NET List project(s) in a solution file Command

Usage: dotnet sln <SLN_FILE> list [options]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information",

            ["dotnet sln remove -h"] =
            @".NET Remove project(s) from a solution file Command

Usage: dotnet sln <SLN_FILE> remove [options] [args]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Additional Arguments:
 Remove the specified project(s) from the solution. The project is not impacted.",

            ["dotnet test -h"] =
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
 "
        };
    }
}