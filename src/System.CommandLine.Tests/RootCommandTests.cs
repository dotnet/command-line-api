// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class RootCommandTests
    {
        [Fact]
        public void Root_command_name_is_executable_name()
        {
            var rootCommand = new RootCommand();

            rootCommand.Name.Should().Be(RootCommand.ExeName);
        }

        [Fact]
        public void Attempting_to_set_root_command_name_throws()
        {
            var rootCommand = new RootCommand();

            rootCommand.Invoking(c => c.Name = "something")
                       .Should()
                       .Throw<NotSupportedException>()
                       .WithMessage("The root command's name cannot be changed.");
        }
    }
}
