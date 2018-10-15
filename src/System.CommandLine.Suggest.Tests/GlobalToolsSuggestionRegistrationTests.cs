// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class GlobalToolsSuggestionRegistrationTests
    {
        [Fact]
        public void Path_is_in_global_tools()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var validToolsPath = Path.Combine(dotnetProfileDirectory, "tools", "play");
            var fileInfo = new FileInfo(validToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
                new FakeFileEnumerator(dotnetProfileDirectory));

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.Value.CommandPath.Should().Be(validToolsPath);
            pair.Value.SuggestionCommand.Should().Be("play [suggest]");
        }

        [Fact]
        public void Invalid_global_tools_returns_null()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var invalidToolsPath = Path.Combine(dotnetProfileDirectory, "not-valid");
            var fileInfo = new FileInfo(invalidToolsPath);
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
                new FakeFileEnumerator(dotnetProfileDirectory));

            var pair = suggestionRegistration.FindRegistration(fileInfo);

            pair.Should().BeNull();
        }

        [Fact]
        public void Global_tools_can_be_found()
        {
            var dotnetProfileDirectory = Path.GetTempPath();
            var suggestionRegistration = new GlobalToolsSuggestionRegistration(dotnetProfileDirectory,
                new FakeFileEnumerator(dotnetProfileDirectory));

            var registrationPairs = suggestionRegistration.FindAllRegistrations();
            registrationPairs.Should()
                .Contain(new RegistrationPair(
                    Path.Combine(dotnetProfileDirectory, "tools", "dotnet-suggest"),
                    "dotnet-suggest [suggest]"));
            registrationPairs.Should()
                .Contain(new RegistrationPair(Path.Combine(dotnetProfileDirectory, "tools", "t-rex"),
                    "t-rex [suggest]"));
        }

        private class FakeFileEnumerator : IFileEnumerator
        {
            private readonly string _dotnetProfileDirectory;

            public FakeFileEnumerator(string homeDir)
            {
                _dotnetProfileDirectory = homeDir ?? throw new ArgumentNullException(nameof(homeDir));
            }

            public IEnumerable<string> EnumerateFiles(string path)
            {
                if (path == Path.Combine(_dotnetProfileDirectory, "tools"))
                {
                    return new[]
                    {
                        Path.Combine(_dotnetProfileDirectory, "tools", "dotnet-suggest"),
                        Path.Combine(_dotnetProfileDirectory, "tools", "t-rex")
                    };
                }

                return Array.Empty<string>();
            }
        }
    }
}
