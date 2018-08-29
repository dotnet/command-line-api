using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{

    class UpdateCommand : ToolCommand
    {
         public UnInstallCommand(bool global, string toolPath, Verbosity verbosity)
           : base(global, toolPath, verbosity)
        { }

        public void Invoke()
        {
            ToolStuff.Update(Global, ToolPath, Verbosity);
        }
    }
}
