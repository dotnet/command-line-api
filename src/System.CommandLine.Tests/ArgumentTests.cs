// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests;

public class ArgumentTests
{
    [Fact]
    public void By_default_there_is_no_default_value()
    {
        var argument = new Argument<string>("arg");

        argument.HasDefaultValue.Should().BeFalse();
    }

    [Fact]
    public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
    {
        var argument = new Argument<string[]>("arg");

        argument.DefaultValueFactory = _ => null;

        argument.HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void When_there_is_no_default_value_then_GetDefaultValue_throws()
    {
        var argument = new Argument<string>("the-arg");

        argument.Invoking(a => a.GetDefaultValue())
                .Should()
                .Throw<InvalidOperationException>()
                .Which
                .Message
                .Should()
                .Be("Argument \"the-arg\" does not have a default value");
    }

    [Fact]
    public void GetRequiredValue_does_not_throw_when_help_is_requested_and_DefaultValueFactory_is_set()
    {
        var argument = new Argument<string>("the-arg")
        {
            DefaultValueFactory = _ => "default"
        };

        var result = new RootCommand { argument }.Parse("-h");

        using var _ = new AssertionScope();

        result.Invoking(r => r.GetRequiredValue(argument)).Should().NotThrow();
        result.GetRequiredValue(argument).Should().Be("default");

        result.Invoking(r => r.GetRequiredValue<string>("the-arg")).Should().NotThrow();
        result.GetRequiredValue<string>("the-arg").Should().Be("default");

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void When_there_is_no_default_value_then_GetDefaultValue_does_not_throw_for_bool()
    {
        var argument = new Argument<bool>("the-arg");

        argument.GetDefaultValue().Should().Be(false);
    }

    [Fact]
    public void When_there_is_no_default_value_then_GetRequiredValue_does_not_throw_for_bool()
    {
        var argument = new Argument<bool>("the-arg");

        var result = new RootCommand { argument }.Parse("");

        using var _ = new AssertionScope();

        result.Invoking(r => r.GetRequiredValue(argument)).Should().NotThrow();
        result.GetRequiredValue(argument).Should().BeFalse();

        result.Invoking(r => r.GetRequiredValue<bool>("the-arg")).Should().NotThrow();
        result.GetRequiredValue<bool>("the-arg").Should().BeFalse();

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Argument_of_enum_can_limit_enum_members_as_valid_values()
    {
        var argument = new Argument<ConsoleColor>("color");
        argument.AcceptOnlyFromAmong(ConsoleColor.Red.ToString(), ConsoleColor.Green.ToString());

        Command command = new("set-color")
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