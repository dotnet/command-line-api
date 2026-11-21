using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class TestSuggestionRegistrationTests : SuggestionRegistrationTest
    {
        protected override ISuggestionRegistration GetSuggestionRegistration() => new TestSuggestionRegistration();

        [Fact]
        public void Suggestion_command_path_is_not_case_sensitive()
        {
            ISuggestionRegistration suggestionProvider = GetSuggestionRegistration();

            suggestionProvider.AddSuggestionRegistration(
                new Registration(Path.GetFullPath("commandPath")));

            Registration registration = suggestionProvider.FindRegistration(new FileInfo("COMMANDPATH"));

            registration.ExecutablePath.Should().Be(Path.GetFullPath("commandPath"));
        }
    }
}
