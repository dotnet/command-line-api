// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class GlobalToolsSuggestionRegistrationTests
    {
        public static IEnumerable<string> FilesNameWithoutExtensionUnderDotnetProfileToolsExample = new[] { "dotnet-suggest", "t-rex" };
        [Fact]
        public void Path_is_in_global_tools()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var validToolsPath = Path.Combine(dotnetProfileDirectory, "tools", "play");
            var fileInfo = new FileInfo(validToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
                FilesNameWithoutExtensionUnderDotnetProfileToolsExample);

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.ExecutablePath.Should().Be(validToolsPath);
        }

        [Fact]
        public void Invalid_global_tools_returns_null()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var invalidToolsPath = Path.Combine(dotnetProfileDirectory, "not-valid");
            var fileInfo = new FileInfo(invalidToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
               FilesNameWithoutExtensionUnderDotnetProfileToolsExample);

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.Should().BeNull();
        }

        [Fact]
        public void Global_tools_can_be_found()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
                                                                               FilesNameWithoutExtensionUnderDotnetProfileToolsExample);

            var registrationPairs = suggestionRegistration.FindAllRegistrations();

            registrationPairs
                .Should()
                .BeEquivalentTo(
                    new Registration(
                        Path.Combine(dotnetProfileDirectory, "tools", "dotnet-suggest")),
                    new Registration(
                        Path.Combine(dotnetProfileDirectory, "tools", "t-rex")));
        }
    }
}
