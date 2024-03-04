// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.Reflection;
using Xunit;

namespace System.CommandLine.Subsystems.Tests
{
    public class PipelineTests
    {

        private static readonly string? version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                 ?.InformationalVersion;


        [Theory]
        [InlineData("-v", true)]
        [InlineData("--version", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Subsystem_runs_in_pipeline_only_when_requested(string input, bool shouldRun)
        {
            var configuration = new CliConfiguration(new CliRootCommand { });
            var pipeline = new Pipeline
            {
                Version = new VersionSubsystem()
            };
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);

            var exit = pipeline.Execute(configuration, input, consoleHack);

            exit.ExitCode.Should().Be(0);
            exit.Handled.Should().Be(shouldRun);
            if (shouldRun)
            {
                consoleHack.GetBuffer().Trim().Should().Be(version);
            }
        }

        [Theory]
        [InlineData("-v", true)]
        [InlineData("--version", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Subsystem_runs_with_explicit_parse_only_when_requested(string input, bool shouldRun)
        {
            var configuration = new CliConfiguration(new CliRootCommand { });
            var pipeline = new Pipeline
            {
                Version = new VersionSubsystem()
            };
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);

            var result = pipeline.Parse(configuration, input);
            var exit = pipeline.Execute(result, input, consoleHack);

            exit.ExitCode.Should().Be(0);
            exit.Handled.Should().Be(shouldRun);
            if (shouldRun)
            {
                consoleHack.GetBuffer().Trim().Should().Be(version);
            }
        }

        [Theory]
        [InlineData("-v", true)]
        [InlineData("--version", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Subsystem_runs_initialize_and_teardown_when_requested(string input, bool shouldRun)
        {
            var configuration = new CliConfiguration(new CliRootCommand { });
            AlternateSubsystems.VersionWithInitializeAndTeardown versionSubsystem = new AlternateSubsystems.VersionWithInitializeAndTeardown();
            var pipeline = new Pipeline
            {
                Version = versionSubsystem
            };
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);

            var exit = pipeline.Execute(configuration, input, consoleHack);

            exit.ExitCode.Should().Be(0);
            exit.Handled.Should().Be(shouldRun);
            versionSubsystem.InitializationWasRun.Should().BeTrue();
            versionSubsystem.ExecutionWasRun.Should().Be(shouldRun);
            versionSubsystem.TeardownWasRun.Should().BeTrue();
        }


        [Theory]
        [InlineData("-v", true)]
        [InlineData("--version", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Subsystem_can_be_used_without_runner(string input, bool shouldRun)
        {
            var configuration = new CliConfiguration(new CliRootCommand { });
            var versionSubsystem = new VersionSubsystem();
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);

            Subsystem.Initialize(versionSubsystem, configuration);
            // TODO: I do not know why anyone would do this, but I do not see a reason to work to block it. See style2 below
            var parseResult = CliParser.Parse(configuration.RootCommand, input, configuration);
            bool value = parseResult.GetValue<bool>("--version");

            value.Should().Be(shouldRun);
            if (shouldRun) 
            {
                // TODO: Add an execute overload to avoid checking activated twice
                var exit = Subsystem.Execute(versionSubsystem, parseResult, input, consoleHack);
                exit.Should().NotBeNull();
                exit.ExitCode.Should().Be(0);
                exit.Handled.Should().BeTrue();
                consoleHack.GetBuffer().Trim().Should().Be(version);
            }
        }

        [Theory]
        [InlineData("-v", true)]
        [InlineData("--version", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Subsystem_can_be_used_without_runner_style2(string input, bool shouldRun)
        {
            var configuration = new CliConfiguration(new CliRootCommand { });
            var versionSubsystem = new VersionSubsystem();
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var expectedVersion = shouldRun
                        ? version
                        : "";

            Subsystem.Initialize(versionSubsystem, configuration);
            var parseResult = CliParser.Parse(configuration.RootCommand, input, configuration);
            var exit = Subsystem.ExecuteIfNeeded(versionSubsystem, parseResult, input, consoleHack);

            exit.ExitCode.Should().Be(0);
            exit.Handled.Should().Be(shouldRun);
            consoleHack.GetBuffer().Trim().Should().Be(expectedVersion);
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
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var symbol = new CliOption<bool>("-x");

            var pipeline = new StandardPipeline
            {
                Version = new AlternateSubsystems.VersionThatUsesHelpData(symbol)
            };
            if (pipeline.Help is null) throw new InvalidOperationException();
            var rootCommand = new CliRootCommand
            {
                symbol.With(pipeline.Help.Description, "Testing")
            };
            pipeline.Execute(new CliConfiguration(rootCommand), "-v", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"Testing");
        }

    }
}
