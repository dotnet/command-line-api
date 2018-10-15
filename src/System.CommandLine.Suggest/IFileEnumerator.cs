using System.Collections.Generic;

namespace System.CommandLine.Suggest
{
    public interface IFileEnumerator
    {
        IEnumerable<string> EnumerateFiles(string path);
    }
}