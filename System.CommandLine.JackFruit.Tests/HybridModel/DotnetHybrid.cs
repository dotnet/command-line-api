using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit.Tests
{
    public class DotnetHybrid
    {
        public class Add
        {
            [Argument]
            public FileInfo ProjectFile { get; }

            public async Task<int> Package(FileInfo projectFile, [Argument] string packageName,
                                    string _framework, string _source, bool _noRestore, bool interactive,
                                    DirectoryInfo PackageDirectory)
                => await Task.FromResult(42); // actually do stuff

            public async Task<int> Reference(FileInfo projectFile, string projectPath, string _framework)
                => await Task.FromResult(42); // actually do stuff
        }

        public class List
        {
            [Argument]
            public FileInfo ProjectFile { get; set; }

            public async Task<int> Package(FileInfo projectFile)
                => await Task.FromResult(42); // actually do stuff

            public async Task<int> Reference(FileInfo projectFile)
                => await Task.FromResult(42); // actually do stuff
        }

        public class Remove
        {
            public FileInfo ProjectFileArg { get; set; }

            public async Task<int> Package(FileInfo projectFile, string packageNameArg)
                => await Task.FromResult(42); // actually do stuff

            public async Task<int> Reference(FileInfo projectFile, FileInfo projectPathArg, string framework)
                => await Task.FromResult(42); // actually do stuff
        }

        public class Tool
        {
            public async Task<int> Install(
                    [Argument] string packageId,
                    [Alias("-g")] bool global,
                    DirectoryInfo toolPath,
                    string version,
                    FileInfo configFile,
                    string addSource,
                    string framework,
                    [Alias("-v")] StandardVerbosity verbosity)
                => await ToolActions.InstallAsync(packageId, global, toolPath, version, configFile,
                            addSource, framework, verbosity);

            public async Task<int> Update(
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath,
                    FileInfo ConfigFile,
                    string AddSource,
                    string Framework,
                    [Alias("-v")] StandardVerbosity Verbosity)
                => await Task.FromResult(42); // actually do stuff

            public async Task<int> List(
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath)
                => await Task.FromResult(42); // actually do stuff

            public async Task<int> Uninstall(
                    [Argument] string PackageId,
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath)
                => await Task.FromResult(42); // actually do stuff
        }

        [Help("Modify Visual Studio solution files.")]
        public class Sln
        {
            [Argument]
            [Help("The solution file to operate on.If not specified, the command will search the current directory for one.")]
            public FileInfo SlnFile { get; set; }

            [Help("Add one or more projects to a solution file.")]
            public async Task<int> Add(
                    [Argument]
                    [Help("The paths to the projects to add to the solution.")]
                    FileInfo ProjectFile)
                => await Task.FromResult(42); // actually do stuff

            [Help("List all projects in a solution file.")]
            public async Task<int> List()
                => await Task.FromResult(42); // actually do stuff

            [Help("Remove one or more projects from a solution file.")]
            public async Task<int> Remove(
                    [Argument]
                    [Help("The paths to the projects to remove from the solution.")]
                    FileInfo ProjectFile)
                => await Task.FromResult(42); // actually do stuff
        }
    }
}
