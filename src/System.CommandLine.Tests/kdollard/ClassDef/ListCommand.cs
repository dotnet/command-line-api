using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{
    [Help, Alias] // alias at least for CLI evolution, backwards compat
    class ListCommand : ProjectCommand
    {
        public ListCommand(string project)
        : base(project)
        {    }

        public void Invoke()
        {
            NuGetStuff.List();
        }
    }
}
