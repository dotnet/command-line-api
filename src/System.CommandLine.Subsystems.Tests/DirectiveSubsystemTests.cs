// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class DirectiveSubsystemTests
{

    // For Boolean tests see DiagramSubsystemTests

    [Theory]
    [ClassData(typeof(TestData.Directive))]
    // TODO: Not sure why these tests are passing
    public void String_directive_supplies_string_or_default_and_is_activated_only_when_requested(
        string input, bool expectedBoolIsActive, bool expectedStringIsActive, string? expectedValue)
    {
        CliRootCommand rootCommand = [new CliCommand("x")];
        var configuration = new CliConfiguration(rootCommand);
        var stringSubsystem = new AlternateSubsystems.StringDirectiveSubsystem();
        var boolSubsystem = new AlternateSubsystems.BooleanDirectiveSubsystem();
        var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();

        Subsystem.Initialize(stringSubsystem, configuration, args);
        Subsystem.Initialize(boolSubsystem, configuration, args);

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var stringIsActive = Subsystem.GetIsActivated(stringSubsystem, parseResult);
        var boolIsActive = Subsystem.GetIsActivated(boolSubsystem, parseResult);
        var actualValue = stringSubsystem.Value;

        boolIsActive.Should().Be(expectedBoolIsActive);
        stringIsActive.Should().Be(expectedStringIsActive);
        actualValue.Should().Be(expectedValue);

    }
}
