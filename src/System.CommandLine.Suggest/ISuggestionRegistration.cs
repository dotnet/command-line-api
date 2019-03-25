using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Suggest
{
    public interface ISuggestionRegistration
    {
        void AddSuggestionRegistration(Registration registration);
        Registration FindRegistration(FileInfo soughtExecutable);
        IEnumerable<Registration> FindAllRegistrations();
    }
}
