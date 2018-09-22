using System.IO;

namespace System.CommandLine.Suggest.Tests
{
    public class FileSuggestionProviderTests : SuggestionRegistrationTest, IDisposable
    {
        protected override ISuggestionRegistration GetSuggestionRegistration() => new FileSuggestionRegistration(_filePath);

        private readonly string _filePath;

        public FileSuggestionProviderTests()
        {
            _filePath = Path.GetFullPath(Path.GetRandomFileName());
        }

        public void Dispose()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
