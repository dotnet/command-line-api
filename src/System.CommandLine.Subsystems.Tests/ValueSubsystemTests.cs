// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ValueSubsystemTests
{
    [Fact]
    public void Value_is_always_activated()
    {
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                new CliOption<string>("--opt1")
            }];
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new ValueSubsystem();
        var input = "x --opt1 Kirk";
        var args = CliParser.SplitCommandLine(input).ToList();

        Subsystem.Initialize(subsystem, configuration, args);
        var parseResult = CliParser.Parse(rootCommand, args[0], configuration);
        var isActive = Subsystem.GetIsActivated(subsystem, parseResult);

        isActive.Should().BeTrue();
    }

    [Fact(Skip ="WIP")]
    public void ValueSubsystem_returns_values_that_are_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        var pipeline = Pipeline.Create();
        CliOption<int> option1 = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                option1
            }];
        var configuration = new CliConfiguration(rootCommand);
        const int expected1 = 42;
        var input = $"x --intValue {expected1}";

        pipeline.Parse(configuration, input);
        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option1).Should().Be(expected1);
    }


    [Fact(Skip = "WIP")]
    public void ValueSubsystem_returns_default_value_when_no_value_is_entered()
    {

    }
}
