namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class TestSuggestionRegistrationTests : SuggestionRegistrationTest
    {
        protected override ISuggestionRegistration GetSuggestionRegistration() => new TestSuggestionRegistration();
    }
}
