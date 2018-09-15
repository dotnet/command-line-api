namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class TestSuggestionProviderTests : SuggestionProviderTests
    {
        protected override ISuggestionProvider GetSuggestionProvider() => new TestSuggestionProvider();
    }
}
