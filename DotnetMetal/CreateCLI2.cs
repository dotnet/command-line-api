using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;

namespace DotnetMetal
{
    public class CreateCli2
    {
        public Parser GetParser()
        {
            IToolActions toolActions = null;
            return new CommandLineBuilder()
                .AddCommand(GetSdkCommand())
                .AddCommand(GetToolCommand(toolActions))
                .Build();
        }

        private Command GetSdkCommand()
        {
            var command = new Command("sdk", Strings.sdkDescription);
            command.AddCommand(new Command("install", Strings.sdkInstallDescription));
            command.AddCommand(new Command("uninstall", Strings.sdkUninstallDescription));
            command.AddCommand(new Command("list", Strings.sdkListDescription));
            command.AddCommand(new Command("update", Strings.sdkUpdateDescription));
            return command;
        }

        private Command GetToolCommand(IToolActions toolActions)
        {
            var command = new Command("tool", Strings.toolDescription);
            command.AddCommand(new Command("install", Strings.toolInstallDescription,
               new Option[] { Global(), ToolPath(), Version(), ConfigFile(), AddSource(), Framework(), StandardVerbosity() },
               handler: CommandHandler.Create<bool, DirectoryInfo, string, FileInfo, string, string, StandardVerbosity>(toolActions.Install)));
            command.AddCommand(new Command("uninstall", Strings.toolUninstallDescription,
               new Option[] { Global(), ToolPath() },
               handler: CommandHandler.Create<bool, DirectoryInfo>(toolActions.Uninstall)));
            command.AddCommand(new Command("list", Strings.toolListDescription,
               new Option[] { Global(), ToolPath() },
                handler: CommandHandler.Create<bool, DirectoryInfo>(toolActions.List)));
            command.AddCommand(new Command("update", Strings.toolUpdateDescription,
               new Option[] { Global(), ToolPath(), ConfigFile(), AddSource(), Framework(), StandardVerbosity() },
               handler: CommandHandler.Create<bool, DirectoryInfo, FileInfo, string, string, StandardVerbosity>(toolActions.Update)));
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
        private Option StandardVerbosity()
             => new Option(new string[] { "--verbosity", "-v" }, Strings.verbosityOptionDescription,
                      new Argument<StandardVerbosity>());

    }

}
