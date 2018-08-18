using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.CompletionSuggestions
{
    public interface ISuggestionFileProvider
    {
        IReadOnlyCollection<string> RegistrationConfigurationFilePaths { get; }
        void AddRegistrationConfigurationFilePath(string configFilePath);
        string FindRegistration(FileInfo soughtExecutable);
        IReadOnlyCollection<string> FindAllRegistrations();
    }
}
