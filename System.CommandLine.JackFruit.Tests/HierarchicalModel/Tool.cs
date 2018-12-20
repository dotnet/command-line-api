using System.CommandLine.JackFruit;
using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit.Tests
{
    internal class Tool : DotnetJackFruit
    { }

    internal class ToolInstall : Tool
    {
        [Argument]
        public string PackageId { get; set; }

        [Alias("-g")]
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        public string Version { get; set; }
        public FileInfo ConfigFile { get; set; }
        public string AddSource { get; set; }
        public string Framework { get; set; }
        [Alias("-v")]
        public StandardVerbosity Verbosity { get; set; }
        public async Task<int> InvokeAsync()
            => await ToolActions.InstallAsync(PackageId, Global, ToolPath, Version,
                  ConfigFile, AddSource, Framework, Verbosity);
    }

    internal class ToolUpdate : Tool
    {
        [Argument]
        public string PackageId { get; set; }

        [Alias("-g")]
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        public FileInfo ConfigFile { get; set; }
        public string AddSource { get; set; }
        public string Framework { get; set; }
        public StandardVerbosity Verbosity { get; set; }
        [Ignore]
        public bool SkipThisOne { get; set; }

        public async Task<int> InvokeAsync()
            => await ToolActions.UpdateAsync(PackageId, Global, ToolPath,
                  ConfigFile, AddSource, Framework, Verbosity);
    }

    internal class ToolList : Tool
    {
        [Alias("-g")]
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }
        public async Task<int> InvokeAsync()
            => await ToolActions.ListAsync(Global, ToolPath);
    }

    internal class ToolUninstall : Tool
    {
        [Argument]
        public string PackageId { get; set; }

        [Alias("-g")]
        public bool Global { get; set; }
        public DirectoryInfo ToolPath { get; set; }

        //public async Task<int> InvokeAsync()
            //=> await ToolActions.UninstallAsync(PackageId, Global, ToolPath);
    }

    internal class ToolUninstallTest : ToolUninstall
    {
        public async Task<int> InvokeAsync()
            => await ToolActions.ListAsync(Global, ToolPath);
    }

    internal class ToolUninstallTest2 : ToolUninstall
    {
        public async Task<int> InvokeAsync()
            => await ToolActions.ListAsync(Global, ToolPath);
    }
}
