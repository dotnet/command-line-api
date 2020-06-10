// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Builder
{
    public class ConfigurableBuilderTests
    {
        [Fact]
        public void When_a_ConfigurableCommandLineBuider_is_created_it_has_a_null_Command()
        {
            var builder = new ConfigurableCommandLineBuilder();
            builder.Command.Should().BeNull();
        }

        [Fact]
        public void ConfigurableCommandLineBuider_Command_can_be_set()
        {
            var builder = new ConfigurableCommandLineBuilder();
            builder.SetCommand(new Command("Bob"));
            builder.Command.Should().NotBeNull()
                .And.BeEmpty("Bob");

        }

        [Fact]
        public void ConfigurableCommandLineBuider_Command_should_throw_if_Command_already_set()
        {
            var builder = new ConfigurableCommandLineBuilder();
            builder.SetCommand(new Command("Bob"));
            Action create = () => builder.SetCommand(new Command("Joe"));

            create.Should()
                      .Throw<InvalidOperationException>()
                      .Which
                      .Message
                      .Should()
                      .Be("Command has already been set.");
        }
    }
}
