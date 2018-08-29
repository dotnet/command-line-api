using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{
    [Help, Alias] // alias at least for CLI evolution, backwards compat
    class ProjectCommand : DotNetCommand
    {  

        public ProjectCommand(string project)
        {
            Project = project;
        }

        public string Project { get;  }
    }
}
