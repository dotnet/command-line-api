using System.IO;
using System.CommandLine.JackFruit;
using System.CommandLine;

namespace JackFruit
{
    internal class Sln : DotnetJackFruit
    {
        [Argument]
        public FileInfo SolutionFile { get; set; }

        internal class Add
        {
            [Argument]
            public FileInfo ProjectFile { get; set; }
        }

        internal class List
        {       }

        internal class Remove
        {
            [Argument]
            public FileInfo ProjectFile { get; set; }
        }
    }
}
