using System.IO;
using System.CommandLine.JackFruit;
using System.CommandLine;

namespace System.CommandLine.JackFruit.Tests
{
    [Help("Modify Visual Studio solution files.")]
    internal class Sln : DotnetJackFruit
    {
        [Argument]
        [Help("The solution file to operate on.If not specified, the command will search the current directory for one.")]
        public FileInfo SlnFile { get; set; }

        [Help("Add one or more projects to a solution file.")]
        internal class Add : Sln 
        {
            [Argument]
            [Help("The paths to the projects to add to the solution.")]
            public FileInfo ProjectFile { get; set; }
        }

        [Help("List all projects in a solution file.")]
        internal class List : Sln
        {       }

        [Help("Remove one or more projects from a solution file.")]
        internal class Remove : Sln
        {
            [Argument]
            [Help("The paths to the projects to remove from the solution.")]
            public FileInfo ProjectFile { get; set; }
        }
    }
}
