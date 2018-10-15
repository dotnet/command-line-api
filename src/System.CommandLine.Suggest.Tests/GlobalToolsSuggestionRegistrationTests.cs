// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class GlobalToolsSuggestionRegistrationTests
    {
        [Fact]
        public void Path_is_in_global_tools()
        {
            var homeDir = Path.GetTempPath();
            var validToolsPath = Path.Combine(homeDir, "tools", "play");
            var fileInfo = new FileInfo(validToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(homeDir);

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.Value.CommandPath.Should().Be(validToolsPath);
            pair.Value.SuggestionCommand.Should().Be("play [suggest]");
        }

        [Fact]
        public void Invalid_global_tools_returns_null()
        {
            var homeDir = Path.GetTempPath();
            var invalidToolsPath = Path.Combine(homeDir, "not-valid");
            var fileInfo = new FileInfo(invalidToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(homeDir);

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.Should().BeNull();
        }
    }
}
