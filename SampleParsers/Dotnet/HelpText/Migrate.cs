// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Migrate
    {
        public const string HelpText =
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
";
    }
}