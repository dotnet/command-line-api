using System.IO;
using System.Threading.Tasks;

namespace JackFruit
{
    internal class Tool : DotnetJackFruit
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
        public async Task<int> Invoke()
        {
            return await ToolActions.InstallAsync(Global, ToolPath, Version,
                  ConfigFile, AddSource, Framework, Verbosity);
        }
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

    }
}
