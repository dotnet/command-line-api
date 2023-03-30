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
            var rootCommand = new CliRootCommand();

            rootCommand.Name.Should().Be(CliRootCommand.ExecutableName);
        }
    }
}
