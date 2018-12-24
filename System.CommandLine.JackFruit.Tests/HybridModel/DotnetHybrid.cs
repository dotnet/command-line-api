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

            public Task<int> Package(FileInfo ProjectFile, string PackageName,
                    string _Framework, string _Source, bool _NoRestore, bool Interactive,
                    DirectoryInfo PackageDirectory)
            {
                return null; // actually do stuff}
            }
            public Task<int> Reference(FileInfo ProjectFile, string ProjectPath,
              string _Framework)
            {
                return null; // actually do stuff                    }
            }
        }

        public class List
        {
            [Argument]
            public FileInfo ProjectFile { get; set; }

            internal class Package
            {
                internal Task<int> InvokeAsync(FileInfo ProjectFile)
                {
                    return null; // actually do stuff                    }
                }
            }

            internal class Reference
            {
                internal Task<int> InvokeAsync(FileInfo ProjectFile)
                {
                    return null; // actually do stuff                    }
                }
            }
        }

        public class Remove
        {
            public FileInfo ProjectFileArg { get; set; }

            internal class Package
            {
                public string PackageNameArg { get; set; }
                internal Task<int> InvokeAsync(FileInfo ProjectFile)
                {
                    return null; // actually do stuff                    }
                }
            }

            internal class Reference
            {
                public FileInfo ProjectPathArg { get; set; }

                public string Framework { get; set; }
            }
        }
    }
}
