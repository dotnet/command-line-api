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
            var homeDir = "/Users/jeredmyers";
            var validToolsPath = "/Users/jeredmyers/.dotnet/tools/play";
            var fileInfo = new FileInfo(validToolsPath);
            var gtRegister = new GlobalToolsSuggestionRegistration(homeDir);

            var pair = gtRegister.FindRegistration(fileInfo);

            pair.CommandPath.Should().Be(validToolsPath);
            pair.SuggestionCommand.Should().Be("[suggest]");
        }

        [Fact]
        public void Invalid_global_tools_returns_null()
        {
            var homeDir = "/Users/jeredmyers";
            var invalidToolsPath = "/Not/a/valid/path";
            var fileInfo = new FileInfo(invalidToolsPath);
            var gtRegister = new GlobalToolsSuggestionRegistration(homeDir);

            var pair = gtRegister.FindRegistration(fileInfo);

            pair.Should().BeNull();
        }
    }
}