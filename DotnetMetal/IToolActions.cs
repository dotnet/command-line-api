using System.IO;

namespace DotnetMetal
{
    public interface IToolActions
    {
        void Install(bool global, DirectoryInfo toolPath, string version, FileInfo file, string addSource, string framework, StandardVerbosity verbosity);
        void Uninstall(bool global, DirectoryInfo toolPath);
        void List(bool global, DirectoryInfo toolPath);
        void Update(bool global, DirectoryInfo toolPath, FileInfo file, string addSource, string framework, StandardVerbosity verbosity);

    }
}
