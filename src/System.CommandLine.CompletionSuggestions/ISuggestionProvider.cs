using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.CompletionSuggestions
{
    public interface ISuggestionProvider
    {
        void AddSuggestionRegistration(SuggestionRegistration registration);
        SuggestionRegistration FindRegistration(FileInfo soughtExecutable);
        IReadOnlyCollection<SuggestionRegistration> FindAllRegistrations();
    }
}
