using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using static DotnetMetal.Strings;

namespace DotnetMetal
{
    public static class CreateCli
    {
        public static CommandLineBuilder ConfigureParser(this CommandLineBuilder builder)
        {
            return builder.AddCommand(Tool())
                          .AddCommand(
                              new Command("--info",
                                          RootInfoOptionDescription,
                                          handler: CommandHandler.Create(InfoHandler)))
                          .AddCommand(
                              new Command("--list-sdks",
                                          RootListSdksOptionDescription,
                                          handler: CommandHandler.Create(ListSdksHandler)))
                          .AddCommand(
                              new Command("--list-runtimes",
                                          RootListRuntimesOptionDescription,
                                          handler: CommandHandler.Create(ListRuntimesHandler)))
                          .AddVersionOption();
        }

        private static void InfoHandler() => Console.WriteLine("Output some info");

        private static void ListSdksHandler() => Console.WriteLine("Output some SDKs");

        private static void ListRuntimesHandler() => Console.WriteLine("Output some runtimes");

        private static Command Tool()
        {
            var command = new Command("tool", ToolDescription);
            
            command.AddCommand(
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
                            handler: CommandHandler.Create<bool, DirectoryInfo, string, FileInfo, string, string, StandardVerbosity>(ToolActions.Install),
                            argument: new Argument<string>()));
            command.AddCommand(
                new Command("uninstall", ToolUninstallDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo>(ToolActions.Uninstall)));
            command.AddCommand(
                new Command("list", ToolListDescription,
                            new Option[]
                            {
                                Global(),
                                ToolPath()
                            },
                            handler: CommandHandler.Create<bool, DirectoryInfo>(ToolActions.List)));
            command.AddCommand(
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
                            handler: CommandHandler.Create<bool, DirectoryInfo, FileInfo, string, string, StandardVerbosity>(ToolActions.Update),
                            argument: new Argument<string>()));

            return command;

            Option Global()
                => new Option(new[] { "--global", "-g" }, ToolGlobalOptionDescription);

            Option ToolPath()
                => new Option("--tool-path", ToolPathOptionDescription,
                              new Argument<DirectoryInfo>().ExistingFilesOnly());

            Option Version()
                => new Option("--version", VersionOptionDescription,
                              new Argument<string>());

            Option ConfigFile()
                => new Option("--configfile", ConfigFileOptionDescription,
                              new Argument<FileInfo>().ExistingFilesOnly());

            Option AddSource()
                => new Option("--add-source", AddSourceOptionDescription,
                              new Argument<string>());

            Option Framework()
                => new Option("--framework", FrameworkOptionDescription,
                              new Argument<string>());
        }

        private static Option StandardVerbosity()
            => new Option(new[] { "--verbosity", "-v" }, VerbosityOptionDescription,
                          new Argument<StandardVerbosity>());
    }
}
