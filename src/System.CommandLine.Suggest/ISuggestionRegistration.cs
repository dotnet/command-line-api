using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Suggest
{
    public interface ISuggestionRegistration
    {
        void AddSuggestionRegistration(RegistrationPair registration);
        RegistrationPair FindRegistration(FileInfo soughtExecutable);
        IEnumerable<RegistrationPair> FindAllRegistrations();
    }
}
