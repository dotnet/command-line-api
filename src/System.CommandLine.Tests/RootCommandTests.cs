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
        public void HelpName_can_be_set_explicitly()
        {
            var rootCommand = new RootCommand
            {
                HelpName = "my-tool"
            };

            rootCommand.HelpName.Should().Be("my-tool");
        }

        [Fact]
        public void HelpName_can_be_set_to_null_explicitly()
        {
            var rootCommand = new RootCommand
            {
                HelpName = null
            };

            rootCommand.HelpName.Should().BeNull();
        }

        [Fact]
        public void Setting_HelpName_does_not_change_Name()
        {
            var rootCommand = new RootCommand
            {
                HelpName = "my-tool"
            };

            rootCommand.Name.Should().Be(RootCommand.ExecutableName);
        }
    }
}
