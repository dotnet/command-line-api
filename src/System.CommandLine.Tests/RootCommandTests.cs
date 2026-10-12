// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class RootCommandTests
    {
        [Theory]
        [InlineData("/x/Contoso.Tool", "Contoso.Tool")]
        [InlineData("./Contoso.Tool", "Contoso.Tool")]
        [InlineData("Contoso.Tool.exe", "Contoso.Tool")]
        [InlineData("/x/Contoso.Tool.dll", "Contoso.Tool")]
        [InlineData("/x/foo.EXE", "foo")]
        [InlineData("/x/My App", "MyApp")]
        [InlineData("/x/foo", "foo")]
        public void Executable_name_only_strips_host_extensions(string path, string expected)
        {
            ExecutableNameHelper.GetExecutableName(path).Should().Be(expected);
        }

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

        [Fact]
        public void ExecutablePath_is_not_null()
        {
            RootCommand.ExecutablePath.Should().NotBeNull();
        }

        [Fact]
        public void ExecutableName_is_not_null_or_empty()
        {
            RootCommand.ExecutableName.Should().NotBeNullOrEmpty();
        }
    }
}
