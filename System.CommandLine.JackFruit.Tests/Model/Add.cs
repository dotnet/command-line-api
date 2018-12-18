using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.JackFruit.Tests
{
    internal class Add : DotnetJackFruit
    {
        public Add(FileInfo ProjectFile )
        {
            this.ProjectFile = ProjectFile;
        }

        public FileInfo ProjectFile { get; }

        internal class Package : Add
        {
            public Package(FileInfo projectFile, string packageName)
                    : base(projectFile)
                => PackageName = packageName;

            public string PackageName { get;  }

            public string _Framework { get; set; }
            public string _Source { get; set; }
            public bool _NoRestore { get; set; }
            public bool Interactive { get; set; }
            public DirectoryInfo PackageDirectory { get; set; }
        }

        internal class Reference : Add
        {
            public Reference(FileInfo projectFile, IEnumerable<FileInfo> projectPath)
                      : base(projectFile)
                  => ProjectPath = projectPath;
            public IEnumerable<FileInfo> ProjectPath { get; set; }

            public string Framework { get; set; }
        }
    }
}
