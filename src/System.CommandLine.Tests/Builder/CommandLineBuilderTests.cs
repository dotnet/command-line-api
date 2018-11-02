// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Builder
{
    public class CommandLineBuilderTests
    {
        [Fact]
        public void An_implicit_root_command_corresponds_to_the_executable()
        {
            var result = new CommandLineBuilder()
                         .AddOption("-x")
                         .AddOption("-y")
                         .Build()
                         .Parse("-x -y");

            var command = result.CommandResult;

            command.Should().NotBeNull();

            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void Child_builders_can_be_accessed_after_being_added()
        {
            var builder = new CommandLineBuilder();

            builder.AddCommand("the-command", "",
                               cmd => cmd.AddOption(new[] { "-o", "--the-option" }));

            var option = builder
                         .Commands["the-command"]
                         .Options["the-option"];

            option.Aliases.Should().BeEquivalentTo("-o", "--the-option");
        }
    }
}
