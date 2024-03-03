// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using FluentAssertions;
using Xunit;
using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Tests
{
    public class VersionSubsystemTests
    {
        private static readonly string? version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                 ?.InformationalVersion;

        private readonly string newLine = Environment.NewLine;


        [Fact]
        public void When_version_subsystem_is_used_the_version_option_is_added_to_the_root()
        {
            var rootCommand = new CliRootCommand
                             {
                                 new CliOption<bool>("-x")
                             };
            var configuration = new CliConfiguration(rootCommand);
            var pipeline = new Pipeline
            {
                Version = new VersionSubsystem()
            };

            // Parse is used because directly calling Initialize would be unusual
            var result = pipeline.Parse(configuration, "");

            rootCommand.Options.Should().NotBeNull();
            rootCommand.Options
                .Count(x => x.Name == "--version")
                .Should()
                .Be(1);

        }

        [Theory]
        [InlineData("--version", true)]
        [InlineData("-v", true)]
        [InlineData("-x", false)]
        [InlineData("", false)]
        public void Version_is_activated_only_when_requested(string input, bool result)
        {
            CliRootCommand rootCommand = new();
            var configuration = new CliConfiguration(rootCommand);
            var versionSubsystem = new VersionSubsystem();
            Subsystem.Initialize(versionSubsystem, configuration);

            var parseResult = CliParser.Parse(rootCommand, input, configuration);
            var isActive = Subsystem.GetIsActivated(versionSubsystem, parseResult);

            isActive.Should().Be(result);
        }

        [Fact]
        public void Outputs_assembly_version()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var versionSubsystem = new VersionSubsystem();
            Subsystem.Execute(versionSubsystem, new PipelineContext(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be(version);
        }

        [Fact]
        public void Outputs_specified_version()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var versionSubsystem = new VersionSubsystem
            {
                SpecificVersion = "42"
            };
            Subsystem.Execute(versionSubsystem, new PipelineContext(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be("42");
        }

        [Fact]
        public void Outputs_assembly_version_when_specified_version_set_to_null()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var versionSubsystem = new VersionSubsystem
            {
                SpecificVersion = null
            };
            Subsystem.Execute(versionSubsystem, new PipelineContext(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be(version);
        }

        [Fact]
        public void Console_output_can_be_tested()
        {
            CliConfiguration configuration = new(new CliRootCommand())
            { };

            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var versionSubsystem = new VersionSubsystem();
            Subsystem.Execute(versionSubsystem, new PipelineContext(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be(version);
        }

        [Fact]
        public void Custom_version_subsystem_can_be_used()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = new Pipeline
            {
                Version = new AlternateSubsystems.Version()
            };
            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-v", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***{version}***");
        }

        [Fact]
        public void Custom_version_subsystem_can_replace_standard()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = new StandardPipeline
            {
                Version = new AlternateSubsystems.Version()
            };
            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-v", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***{version}***");
        }
    }
}
