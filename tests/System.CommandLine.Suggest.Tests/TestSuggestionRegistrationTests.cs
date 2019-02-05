namespace System.CommandLine.Suggest.Tests
{
    public class TestSuggestionRegistrationTests : SuggestionRegistrationTest
    {
        protected override ISuggestionRegistration GetSuggestionRegistration() => new TestSuggestionRegistration();
    }
}
