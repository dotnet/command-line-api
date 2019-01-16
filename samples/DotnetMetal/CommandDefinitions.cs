using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using static DotnetMetal.Strings;

namespace DotnetMetal
{
    public static class CommandDefinitions
    {
        public static Command Tool()
        {
            var tool = new Command("tool", ToolDescription);

            tool.AddCommand(List());
            tool.AddCommand(Install());
            tool.AddCommand(Uninstall());
            tool.AddCommand(Update());

            return tool;

            Command List() =>
                new Command("list", ToolListDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo>(
                                ToolActions.List));

            Command Install() =>
                new Command("install", ToolInstallDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath(),
                                Version(),
                                ConfigFile(),
                                AddSource(),
                                Framework(),
                                StandardVerbosity()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo, string, FileInfo,
                                string, string, StandardVerbosity>(ToolActions.Install),
                            argument: new Argument<string>() { Name = "install" });

            Command Update() =>
                new Command("update", ToolUpdateDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath(),
                                ConfigFile(),
                                AddSource(),
                                Framework(),
                                StandardVerbosity()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo, FileInfo, string,
                                string, StandardVerbosity>(ToolActions.Update),
                            argument: new Argument<string>() { Name = "update" });

            Command Uninstall() =>
                new Command("uninstall", ToolUninstallDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo>(ToolActions.Uninstall));

            Option Global() =>
                new Option(new[] { "--global", "-g" }, ToolGlobalOptionDescription);

            Option Version() =>
                new Option("--version", VersionOptionDescription,
                           new Argument<string>() { Name = "version" });

            Option ConfigFile() =>
                new Option("--configfile", ConfigFileOptionDescription,
                           new Argument<FileInfo>(){ Name = "configfile" }.ExistingOnly());

            Option AddSource() =>
                new Option("--add-source", AddSourceOptionDescription,
                           new Argument<string>() { Name = "add-source" });

            Option Framework() =>
                new Option("--framework", FrameworkOptionDescription,
                           new Argument<string>() { Name = "framework" });

            Option ToolPath() =>
                new Option("--tool-path", ToolPathOptionDescription,
                           new Argument<DirectoryInfo>() { Name = "tool-path" }.ExistingOnly());
        }

        private static Option StandardVerbosity()
            => new Option(new[] { "--verbosity", "-v" }, VerbosityOptionDescription,
                          new Argument<StandardVerbosity>() { Name = "verbosity" });
    }
}
