// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
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

        [Fact]
        public void ExecutablePath_falls_back_to_empty_string_when_command_line_args_are_empty()
        {
            GetExecutablePath(Array.Empty<string>()).Should().Be("");
        }

        [Fact]
        public void ExecutablePath_uses_first_command_line_arg()
        {
            GetExecutablePath(new[] { "my-tool", "--help" }).Should().Be("my-tool");
        }

        private static string GetExecutablePath(string[] commandLineArgs)
            => (string)typeof(RootCommand)
                .GetMethod("GetExecutablePath", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { commandLineArgs })!;
    }
}
