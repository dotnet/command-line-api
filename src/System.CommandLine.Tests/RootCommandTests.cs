// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class RootCommandTests
    {
        [Fact]
        public void Root_command_name_defaults_to_executable_name()
        {
            var rootCommand = new RootCommand();

            rootCommand.Name.Should().Be(RootCommand.ExecutableName);
        }

        [Fact]
        public void When_Name_is_set_then_executable_name_is_still_an_alias()
        {
            var rootCommand = new RootCommand();
            rootCommand.Name = "custom";

            rootCommand.Aliases.Should().BeEquivalentTo("custom", RootCommand.ExecutableName);
            rootCommand.Aliases.Should().BeEquivalentTo("custom", RootCommand.ExecutableName);
        }
    }
}
