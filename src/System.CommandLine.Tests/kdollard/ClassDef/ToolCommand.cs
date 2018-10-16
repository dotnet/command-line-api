using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{
    [Help, Alias] // alias at least for CLI evolution, backwards compat
    class ToolCommand : DotNetCommand
    {

        public ToolCommand(bool global, string toolPath, Verbosity verbosity)
            : base()
        {
            Global = global;
            ToolPath = toolPath;
            Verbosity = verbosity;
        }

        public bool Global { get; }
        public string ToolPath { get; }
        public Verbosity Verbosity { get; }


    }
}
