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
        CliRootCommand rootCommand = [new CliCommand("x")];
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new ValueSubsystem();
        string[] args = ["x"];

        Subsystem.Initialize(subsystem, configuration, args);
        var parseResult = CliParser.Parse(rootCommand, args[0], configuration);
        var isActive = Subsystem.GetIsActivated(subsystem, parseResult);

        isActive.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(TestData.Value))]
    public void Diagram_is_activated_only_when_requested(string input, bool expectedIsActive)
    {
        CliRootCommand rootCommand = [new CliCommand("x")];
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new DiagramSubsystem();
        var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();

        Subsystem.Initialize(subsystem, configuration, args);
        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var isActive = Subsystem.GetIsActivated(subsystem, parseResult);

        isActive.Should().Be(expectedIsActive);
    }
}
