// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandLineBuilderTests
    {
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
