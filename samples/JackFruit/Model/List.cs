using System.CommandLine.JackFruit;
using System.IO;

namespace JackFruit
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
