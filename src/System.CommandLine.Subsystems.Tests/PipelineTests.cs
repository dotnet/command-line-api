// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class PipelineTests
{
    private static Pipeline GetTestPipeline(VersionSubsystem versionSubsystem)
        => new()
        {
            Version = versionSubsystem
        };
    private static CliConfiguration GetNewTestConfiguration()
       => new(new CliRootCommand { new CliOption<bool>("-x") }); // Add option expected by test data

    private static ConsoleHack GetNewTestConsole()
        => new ConsoleHack().RedirectToBuffer(true);

    //private static (Pipeline pipeline, CliConfiguration configuration, ConsoleHack consoleHack) StandardObjects(VersionSubsystem versionSubsystem)
    //{
    //    var configuration = new CliConfiguration(new CliRootCommand { new CliOption<bool>("-x") });
    //    var pipeline = new Pipeline
    //    {
    //        Version = versionSubsystem
    //    };
    //    var consoleHack = new ConsoleHack().RedirectToBuffer(true);
    //    return (pipeline, configuration, consoleHack);
    //}

    [Theory]
    [ClassData(typeof(TestData.Version))]
    public void Subsystem_runs_in_pipeline_only_when_requested(string input, bool shouldRun)
    {
        var pipeline = GetTestPipeline(new VersionSubsystem());
        var console = GetNewTestConsole();

        var exit = pipeline.Execute(GetNewTestConfiguration(), input, console);

        exit.ExitCode.Should().Be(0);
        exit.Handled.Should().Be(shouldRun);
        if (shouldRun)
        {
            console.GetBuffer().Trim().Should().Be(TestData.AssemblyVersionString);
        }
    }

    [Theory]
    [ClassData(typeof(TestData.Version))]
    public void Subsystem_runs_with_explicit_parse_only_when_requested(string input, bool shouldRun)
    {
        var pipeline = GetTestPipeline(new VersionSubsystem());
        var console = GetNewTestConsole();

        var result = pipeline.Parse(GetNewTestConfiguration(), input);
        var exit = pipeline.Execute(result, input, console);

        exit.ExitCode.Should().Be(0);
        exit.Handled.Should().Be(shouldRun);
        if (shouldRun)
        {
            console.GetBuffer().Trim().Should().Be(TestData.AssemblyVersionString);
        }
    }

    [Theory]
    [ClassData(typeof(TestData.Version))]
    public void Subsystem_runs_initialize_and_teardown_when_requested(string input, bool shouldRun)
    {
        var versionSubsystem = new AlternateSubsystems.VersionWithInitializeAndTeardown();
        var pipeline = GetTestPipeline(versionSubsystem);
        var console = GetNewTestConsole();

        var exit = pipeline.Execute(GetNewTestConfiguration(), input, console);

        exit.ExitCode.Should().Be(0);
        exit.Handled.Should().Be(shouldRun);
        versionSubsystem.InitializationWasRun.Should().BeTrue();
        versionSubsystem.ExecutionWasRun.Should().Be(shouldRun);
        versionSubsystem.TeardownWasRun.Should().BeTrue();
    }


    [Theory]
    [ClassData(typeof(TestData.Version))]
    public void Subsystem_works_without_pipeline(string input, bool shouldRun)
    {
        var versionSubsystem = new VersionSubsystem();
        // TODO: Ensure an efficient conversion as people may copy this code
        var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();
        var console = GetNewTestConsole();
        var configuration = GetNewTestConfiguration();

        Subsystem.Initialize(versionSubsystem, configuration, args);
        // This approach might be taken if someone is using a subsystem just for initialization
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        bool value = parseResult.GetValue<bool>("--version");

        parseResult.Errors.Should().BeEmpty();
        value.Should().Be(shouldRun);
        if (shouldRun)
        {
            // TODO: Add an execute overload to avoid checking activated twice
            var exit = Subsystem.Execute(versionSubsystem, parseResult, input, console);
            exit.Should().NotBeNull();
            exit.ExitCode.Should().Be(0);
            exit.Handled.Should().BeTrue();
            console.GetBuffer().Trim().Should().Be(TestData.AssemblyVersionString);
        }
    }

    [Theory]
    [ClassData(typeof(TestData.Version))]
    public void Subsystem_works_without_pipeline_style2(string input, bool shouldRun)
    {
        var versionSubsystem = new VersionSubsystem();
        var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();
        var console = GetNewTestConsole();
        var configuration = GetNewTestConfiguration();
        var expectedVersion = shouldRun
                    ? TestData.AssemblyVersionString
                    : "";

        // Someone might use this approach if they wanted to do something with the ParseResult
        Subsystem.Initialize(versionSubsystem, configuration, args);
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        var exit = Subsystem.ExecuteIfNeeded(versionSubsystem, parseResult, input, console);

        exit.ExitCode.Should().Be(0);
        exit.Handled.Should().Be(shouldRun);
        console.GetBuffer().Trim().Should().Be(expectedVersion);
    }


    [Theory]
    [InlineData("-xy", false)]
    [InlineData("--versionx", false)]
    public void Subsystem_runs_when_requested_even_when_there_are_errors(string input, bool shouldRun)
    {
        var versionSubsystem = new VersionSubsystem();
        var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();
        var configuration = GetNewTestConfiguration();

        Subsystem.Initialize(versionSubsystem, configuration, args);
        // This approach might be taken if someone is using a subsystem just for initialization
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        bool value = parseResult.GetValue<bool>("--version");

        parseResult.Errors.Should().NotBeEmpty();
        value.Should().Be(shouldRun);
    }

    [Fact]
    public void Standard_pipeline_contains_expected_subsystems()
    {
        var pipeline = new StandardPipeline();
        pipeline.Version.Should().BeOfType<VersionSubsystem>();
        pipeline.Help.Should().BeOfType<HelpSubsystem>();
        pipeline.ErrorReporting.Should().BeOfType<ErrorReportingSubsystem>();
        pipeline.Completion.Should().BeOfType<CompletionSubsystem>();
    }

    [Fact]
    public void Normal_pipeline_contains_no_subsystems()
    {
        var pipeline = new Pipeline();
        pipeline.Version.Should().BeNull();
        pipeline.Help.Should().BeNull();
        pipeline.ErrorReporting.Should().BeNull();
        pipeline.Completion.Should().BeNull();
    }

    [Fact]
    public void Subsystems_can_access_each_others_data()
    {
        // TODO: Explore a mechanism that doesn't require the reference to retrieve data, this shows that it is awkward
        var symbol = new CliOption<bool>("-x");
        var console = GetNewTestConsole();
        var pipeline = new StandardPipeline
        {
            Version = new AlternateSubsystems.VersionThatUsesHelpData(symbol)
        };
        if (pipeline.Help is null) throw new InvalidOperationException();
        var rootCommand = new CliRootCommand
        {
            symbol.With(pipeline.Help.Description, "Testing")
        };

        pipeline.Execute(new CliConfiguration(rootCommand), "-v", console);

        console.GetBuffer().Trim().Should().Be($"Testing");
    }
}
