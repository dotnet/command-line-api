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

    [Fact]
    public void ValueSubsystem_returns_values_that_are_entered()
    {
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                new CliOption<int>("--intValue"),
                new CliOption<string>("--stringValue"),
                new CliOption<bool>("--boolValue")
            }];
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new ValueSubsystem();
        const int expected1 = 42;
        const string expected2 = "43";
        var input = $"x --intValue {expected1} --stringValue \"{expected2}\" --boolValue";
        var args = CliParser.SplitCommandLine(input).ToList();

        Subsystem.Initialize(subsystem, configuration, args);
        var parseResult = CliParser.Parse(rootCommand, input, configuration);

        parseResult.GetValue<int>("--intValue").Should().Be(expected1);
        parseResult.GetValue<string>("--stringValue").Should().Be(expected2);
        parseResult.GetValue<bool>("--boolValue").Should().Be(true);
    }
}
