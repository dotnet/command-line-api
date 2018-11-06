using System.IO;

namespace JackFruit
{
    internal class DotJackFruit
    { }

    internal class Tool : DotJackFruit
    { }

    internal class ToolInstall : Tool
    {
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        public string Version { get; set; }
        public FileInfo ConfigFile { get; set; }
        public string AddSource { get; set; }
        public string Framework { get; set; }
        public StandardVerbosity Verbosity { get; set; }
    }

    internal class ToolUpgrade : Tool
    {
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        public FileInfo ConfigFile { get; set; }
        public string AddSource { get; set; }
        public string Framework { get; set; }
        public StandardVerbosity Verbosity { get; set; }
    }

    internal class ToolList : Tool
    {
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
    }

    internal class ToolUninstall : Tool
    {
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        invoke()
        {
            ToolActions.Install(toolInstall.Global, toolInstall.ToolPath, toolInstall.Version,
                  toolInstall.ConfigFile, toolInstall.AddSource, toolInstall.Framework, toolInstall.Verbosity);
        }
    }
}
