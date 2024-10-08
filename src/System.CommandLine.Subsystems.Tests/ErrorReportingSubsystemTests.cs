// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ErrorReportingSubsystemTests
{
    [Fact]
    public void Report_when_single_error_writes_to_console_hack()
    {
        var error = new CliDiagnostic(new("", "", "a sweet error message", CliDiagnosticSeverity.Warning, null), []);
        var errors = new List<CliDiagnostic> { error };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be(error.Message);
    }

    [Fact]
    public void Report_when_multiple_error_writes_to_console_hack()
    {
        var error = new CliDiagnostic(new("", "", "a sweet error message", CliDiagnosticSeverity.Warning, null), []);
        var anotherError = new CliDiagnostic(new("", "", "another sweet error message", CliDiagnosticSeverity.Warning, null), []);
        var errors = new List<CliDiagnostic> { error, anotherError };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be($"{error.Message}{Environment.NewLine}{anotherError.Message}");
    }

    [Fact]
    public void Report_when_no_errors_writes_nothing_to_console_hack()
    {
        var errors = new List<CliDiagnostic> { };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be("");
    }

    [Theory]
    [InlineData("-x")]
    [InlineData("-non_existant_option")]
    public void GetIsActivated_GivenInvalidInput_SubsystemIsActive(string input)
    {
        var rootCommand = new CliRootCommand { new CliOption<bool>("-v") };
        var configuration = new CliConfiguration(rootCommand);
        var errorSubsystem = new ErrorReportingSubsystem();
        IReadOnlyList<string> args = [""];
        Subsystem.Initialize(errorSubsystem, configuration, args);

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var isActive = Subsystem.GetIsActivated(errorSubsystem, parseResult);

        isActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("")]
    public void GetIsActivated_GivenValidInput_SubsystemShouldNotBeActive(string input)
    {
        var rootCommand = new CliRootCommand { new CliOption<bool>("-v") };
        var configuration = new CliConfiguration(rootCommand);
        var errorSubsystem = new ErrorReportingSubsystem();
        IReadOnlyList<string> args = [""];
        Subsystem.Initialize(errorSubsystem, configuration, args);

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var isActive = Subsystem.GetIsActivated(errorSubsystem, parseResult);

        isActive.Should().BeFalse();
    }
}
