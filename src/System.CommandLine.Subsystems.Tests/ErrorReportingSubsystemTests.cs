// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Tests;

public class ErrorReportingSubsystemTests
{
    [Fact]
    public void Report_when_single_error_writes_to_console_hack()
    {
        var error = new ParseError("a sweet error message");
        var errors = new List<ParseError> { error };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be(error.Message);
    }

    [Fact]
    public void Report_when_multiple_error_writes_to_console_hack()
    {
        var error = new ParseError("a sweet error message");
        var anotherError = new ParseError("another sweet error message");
        var errors = new List<ParseError> { error, anotherError };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be($"{error.Message}{Environment.NewLine}{anotherError.Message}");
    }

    [Fact]
    public void Report_when_no_errors_writes_nothing_to_console_hack()
    {
        var errors = new List<ParseError> { };
        var errorSubsystem = new ErrorReportingSubsystem();
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);

        errorSubsystem.Report(consoleHack, errors);

        consoleHack.GetBuffer().Trim().Should().Be("");
    }

    [Theory]
    [InlineData("-v", false)]
    [InlineData("-x", true)]
    [InlineData("", false)]
    public void GetIsActivated_tests(string input, bool result)
    {
        var rootCommand = new CliRootCommand {new CliOption<bool>("-v")};
        var configuration = new CliConfiguration(rootCommand);
        var errorSubsystem = new ErrorReportingSubsystem();
        IReadOnlyList<string> args = [""];
        Subsystem.Initialize(errorSubsystem, configuration, args);

        var parseResult = CliParser.Parse(rootCommand, input, configuration);
        var isActive = Subsystem.GetIsActivated(errorSubsystem, parseResult);

        isActive.Should().Be(result);
    }
}
