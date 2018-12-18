using System.CommandLine.JackFruit;
using System.IO;

namespace System.CommandLine.JackFruit.Tests
{
    internal class List : DotnetJackFruit
    {
        [Argument]
        public FileInfo ProjectFile { get; set; }

        internal class Package : List
        { }

        internal class Reference : List
        { }
    }
}
