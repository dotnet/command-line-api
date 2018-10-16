using System.Collections.Generic;

namespace System.CommandLine.Tests.kdollard.ClassDef
{

    class AddCommand : ProjectCommand
    {
        public AddCommand(string projectName)
        : base(projectName)
        {      }

        // For arguments, always null, and no abbrev
        // Argument names get expanded to 'PROJECT_NAME', option names to 'projectName'
           string ProjectName { get; }

        public class PackageCommand : AddCommand
        {
            public PackageCommand(string projectName, string packageId)
              : base(projectName)
            {
                PackageId = packageId;
            }
            public string PackageId { get; }
            public void Invoke()
            {
                NuGetStuff.AddPackage(PackageId);
            }
        }

        public class ReferenceCommand : AddCommand
        {
                  public ReferenceCommand(string projectName, string project2ProjectPath)
              : base(projectName)
            {
                Project2ProjectPath = project2ProjectPath;
            }

            public string Project2ProjectPath { get; }
            public void Invoke()
            {
               Project2ProjectStuff.AddReference(Project2ProjectPath);
            }
        }


    }
}
