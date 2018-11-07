using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;

namespace DotMetal
{
    public static class CreateCli
    {

        public static CommandLineBuilder GetParserBuilder()
        {
            var builder =  new CommandLineBuilder()
                .AddCommand(new Command("--info", Strings.rootInfoOptionDescription,handler: CommandHandler.Create(InfoHandler)))
                .AddOption("--list-sdks", Strings.rootListSdksOptionDescription)
                .AddOption("--list-runtimes", Strings.rootListRuntimesOptionDescription)
                .AddVersionOption()
                .AddCommand(GetToolCommand());
            return builder;
        }

        private static void InfoHandler() => Console.WriteLine("Output some info");

        private static Command GetToolCommand()
        {
            var command = new Command("tool", Strings.toolDescription);
            command.AddCommand(new Command("install", Strings.toolInstallDescription,
                new Option[] { Global(), ToolPath(), Version(), ConfigFile(), AddSource(), Framework(), StandardVerbosity() },
                handler: CommandHandler.Create<bool, DirectoryInfo, string, FileInfo, string, string, StandardVerbosity>(ToolActions.Install),
                argument: new Argument<string>()));
            command.AddCommand(new Command("uninstall", Strings.toolUninstallDescription,
                new Option[] { Global(), ToolPath() },
                handler: CommandHandler.Create<bool, DirectoryInfo>(ToolActions.Uninstall)));
            command.AddCommand(new Command("list", Strings.toolListDescription,
                new Option[] { Global(), ToolPath() },
                handler: CommandHandler.Create<bool, DirectoryInfo>(ToolActions.List)));
            command.AddCommand(new Command("update", Strings.toolUpdateDescription,
                new Option[] { Global(), ToolPath(), ConfigFile(), AddSource(), Framework(), StandardVerbosity() },
                handler: CommandHandler.Create<bool, DirectoryInfo, FileInfo, string, string, StandardVerbosity>(ToolActions.Update),
                argument: new Argument<string>()));
            return command;

            Option Global()
              => new Option(new string[] { "--global", "-g" }, Strings.toolGlobalOptionDescription);
            Option ToolPath()
               => new Option("--tool-path", Strings.toolPathOptionDescription,
                      new Argument<DirectoryInfo>().ExistingFilesOnly());
            Option Version()
               => new Option("--version", Strings.versionOptionDescription,
                      new Argument<string>());
            Option ConfigFile()
               => new Option("--configfile", Strings.configFileOptionDescription,
                      new Argument<FileInfo>().ExistingFilesOnly());
            Option AddSource()
                => new Option("--add-source", Strings.addSourceOptionDescription,
                      new Argument<string>());
            Option Framework()
              => new Option("--framework", Strings.frameworkOptionDescription,
                      new Argument<string>());
        }
        private static Option StandardVerbosity()
             => new Option(new string[] { "--verbosity", "-v" }, Strings.verbosityOptionDescription,
                      new Argument<StandardVerbosity>());

    }

}
