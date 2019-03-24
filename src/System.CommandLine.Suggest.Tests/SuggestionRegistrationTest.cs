// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            var suggestion1 = new Registration("commandPath1");
            var suggestion2 = new Registration("commandPath2");

            suggestionProvider.AddSuggestionRegistration(suggestion1);
            suggestionProvider.AddSuggestionRegistration(suggestion2);

            var allRegistrations = suggestionProvider.FindAllRegistrations();
            allRegistrations
                .Should()
                .HaveCount(2)
                .And
                .Contain(x =>
                    x.ExecutablePath == suggestion1.ExecutablePath)
                .And
                .Contain(x =>
                    x.ExecutablePath == suggestion2.ExecutablePath);
        }

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
