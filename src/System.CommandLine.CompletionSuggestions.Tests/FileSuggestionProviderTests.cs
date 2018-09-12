using System.IO;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class FileSuggestionProviderTests : SuggestionProviderTests, IDisposable
    {
        protected override ISuggestionProvider GetSuggestionProvider() => new FileSuggestionProvider(_filePath);

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