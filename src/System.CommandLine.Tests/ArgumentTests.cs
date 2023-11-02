// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests;

public class ArgumentTests
{
    [Fact]
    public void By_default_there_is_no_default_value()
    {
        var argument = new CliArgument<string>("arg");

        argument.HasDefaultValue.Should().BeFalse();
    }

    [Fact]
    public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
    {
        var argument = new CliArgument<string[]>("arg");

        argument.DefaultValueFactory = _ => null;

        argument.HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void When_there_is_no_default_value_then_GetDefaultValue_throws()
    {
        var argument = new CliArgument<string>("the-arg");

        argument.Invoking(a => a.GetDefaultValue())
                .Should()
                .Throw<InvalidOperationException>()
                .Which
                .Message
                .Should()
                .Be("Argument \"the-arg\" does not have a default value");
    }

    [Fact]
    public void Argument_of_enum_can_limit_enum_members_as_valid_values()
    {
        var argument = new CliArgument<ConsoleColor>("color");
        argument.AcceptOnlyFromAmong(ConsoleColor.Red.ToString(), ConsoleColor.Green.ToString());

        CliCommand command = new("set-color")
        {
            argument
        };

        var result = command.Parse("set-color Fuschia");

        result.Errors
              .Select(e => e.Message)
              .Should()
              .BeEquivalentTo(new[] { $"Argument 'Fuschia' not recognized. Must be one of:\n\t'Red'\n\t'Green'" });
    }
}