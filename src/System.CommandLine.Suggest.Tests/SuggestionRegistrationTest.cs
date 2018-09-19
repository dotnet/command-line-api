using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public abstract class SuggestionRegistrationTest
    {
        protected abstract ISuggestionRegistration GetSuggestionRegistration();

        [Fact]
        public void Added_suggestions_can_be_retrieved()
        {
            ISuggestionRegistration suggestionProvider = GetSuggestionRegistration();

            var suggestion1 = new SuggestionRegistration("commandPath1", "suggestionCommand1");
            var suggestion2 = new SuggestionRegistration("commandPath2", "suggestionCommand2");

            suggestionProvider.AddSuggestionRegistration(suggestion1);
            suggestionProvider.AddSuggestionRegistration(suggestion2);

            IReadOnlyCollection<SuggestionRegistration> allRegistrations = suggestionProvider.FindAllRegistrations();
            allRegistrations
                .Should()
                .HaveCount(2)
                .And
                .Contain(x =>
                    x.CommandPath == suggestion1.CommandPath && x.SuggestionCommand == suggestion1.SuggestionCommand)
                .And
                .Contain(x =>
                    x.CommandPath == suggestion2.CommandPath && x.SuggestionCommand == suggestion2.SuggestionCommand);
        }

        [Fact]
        public void Suggestion_command_path_is_not_case_sensitive()
        {
            ISuggestionRegistration suggestionProvider = GetSuggestionRegistration();

            suggestionProvider.AddSuggestionRegistration(
                new SuggestionRegistration(Path.GetFullPath("commandPath"), "suggestionCommand"));

            SuggestionRegistration registration = suggestionProvider.FindRegistration(new FileInfo("COMMANDPATH"));

            registration.CommandPath.Should().Be(Path.GetFullPath("commandPath"));
            registration.SuggestionCommand.Should().Be("suggestionCommand");
        }

        [Fact]
        public void When_duplicate_suggestions_are_registered_the_last_one_is_used()
        {
            ISuggestionRegistration suggestionProvider = GetSuggestionRegistration();

            suggestionProvider.AddSuggestionRegistration(
                new SuggestionRegistration(Path.GetFullPath("commandPath"), "suggestionCommand2"));

            suggestionProvider.AddSuggestionRegistration(
                new SuggestionRegistration(Path.GetFullPath("commandPath"), "suggestionCommand2"));

            SuggestionRegistration registration = suggestionProvider.FindRegistration(new FileInfo("commandPath"));

            registration.CommandPath.Should().Be(Path.GetFullPath("commandPath"));
            registration.SuggestionCommand.Should().Be("suggestionCommand2");
        }
    }

}
