using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{

    class UnInstallCommand : ToolCommand
    {
        public UnInstallCommand(bool global, string toolPath, string version, string configfile,
                string sourceFeed, string framework, Verbosity verbosity)
        : base()
        {
            Global = global;
            ToolPath = toolPath;
            Version = version;
            Configfile = configfile;
            SourceFeed = sourceFeed;
            Framework = framework;
            Verbosity = verbosity;
        }

        public bool Global { get; }
        public string ToolPath { get; }
        public string Version { get; }
        public string Configfile { get; }
        public string SourceFeed { get; }
        public string Framework { get; }
        public Verbosity Verbosity { get; }

        public void Invoke()
        {
            ToolStuff.Install(Global, ToolPath, Version, Configfile, SourceFeed, Framework, Verbosity);
        }
    }
}
