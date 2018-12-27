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

            public Task<int> Package(FileInfo projectFile, string packageName,
                                    string _framework, string _source, bool _noRestore, bool interactive,
                                    DirectoryInfo PackageDirectory)
                => null; // actually do stuff

            public Task<int> Reference(FileInfo projectFile, string projectPath, string _framework)
                => null; // actually do stuff
        }

        public class List
        {
            [Argument]
            public FileInfo ProjectFile { get; set; }

            public Task<int> Package(FileInfo projectFile)
                => null; // actually do stuff

            public Task<int> Reference(FileInfo projectFile)
                => null; // actually do stuff
        }

        public class Remove
        {
            public FileInfo ProjectFileArg { get; set; }

            public Task<int> Package(FileInfo projectFile, string packageNameArg)
                => null; // actually do stuff

            public Task<int> Reference(FileInfo projectFile, FileInfo projectPathArg, string framework)
                => null; // actually do stuff
        }

        public class Tool
        {
            public Task<int> Install(
                    [Argument] string PackageId,
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath,
                    string Version,
                    FileInfo ConfigFile,
                    string AddSource,
                    string Framework,
                    [Alias("-v")] StandardVerbosity Verbosity)
                => null; // actually do stuff

            public Task<int> Update(
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath,
                    FileInfo ConfigFile,
                    string AddSource,
                    string Framework,
                    [Alias("-v")] StandardVerbosity Verbosity)
                => null; // actually do stuff

            public Task<int> List(
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath)
                => null; // actually do stuff

            public Task<int> Uninstall(
                    [Argument] string PackageId,
                    [Alias("-g")] bool Global,
                    DirectoryInfo ToolPath)
                 => null; // actually do stuff
        }

        [Help("Modify Visual Studio solution files.")]
        public class Sln
        {
            [Argument]
            [Help("The solution file to operate on.If not specified, the command will search the current directory for one.")]
            public FileInfo SlnFile { get; set; }

            [Help("Add one or more projects to a solution file.")]
            public Task<int> Add(
                    [Argument]
                    [Help("The paths to the projects to add to the solution.")]
                    FileInfo ProjectFile)
                => null; // actually do stuff

            [Help("List all projects in a solution file.")]
            public Task<int> List()
                => null; // actually do stuff

            [Help("Remove one or more projects from a solution file.")]
            public Task<int> Remove(
                    [Argument]
                    [Help("The paths to the projects to remove from the solution.")]
                    FileInfo ProjectFile)
                => null; // actually do stuff
        }
    }
}
