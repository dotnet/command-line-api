// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Tests
{
    public class VersionSubsystemTests
    {
        [Fact]
        public void When_version_subsystem_is_used_the_version_option_is_added_to_the_root()
        {
            var rootCommand = new CliRootCommand
            {
                new CliOption<bool>("-x") // add option that is expected for the test data used here
            };
            var configuration = new CliConfiguration(rootCommand);
            var pipeline = Pipeline.CreateEmpty();
            pipeline.Version = new VersionSubsystem();

            // Parse is used because directly calling Initialize would be unusual
            var result = pipeline.Parse(configuration, "");

            rootCommand.Options.Should().NotBeNull();
            rootCommand.Options
                .Count(x => x.Name == "--version")
                .Should()
                .Be(1);
        }

        [Theory]
        [ClassData(typeof(TestData.Version))]
        public void Version_is_activated_only_when_requested(string input, bool result)
        {
            CliRootCommand rootCommand = [new CliOption<bool>("-x")]; // add random option as empty CLIs are rare
            var configuration = new CliConfiguration(rootCommand);
            var versionSubsystem = new VersionSubsystem();
            var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();

            Subsystem.Initialize(versionSubsystem, configuration, args);

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
            consoleHack.GetBuffer().Trim().Should().Be(Constants.version);
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
            consoleHack.GetBuffer().Trim().Should().Be(Constants.version);
        }

        [Fact]
        public void Console_output_can_be_tested()
        {
            CliConfiguration configuration = new(new CliRootCommand())
            { };

            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var versionSubsystem = new VersionSubsystem();
            Subsystem.Execute(versionSubsystem, new PipelineContext(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be(Constants.version);
        }

        [Fact]
        public void Custom_version_subsystem_can_be_used()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = Pipeline.CreateEmpty();
            pipeline.Version = new AlternateSubsystems.AlternateVersion();

            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-v", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***{Constants.version}***");
        }

        [Fact]
        public void Custom_version_subsystem_can_replace_standard()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = Pipeline.CreateEmpty();
            pipeline.Version = new AlternateSubsystems.AlternateVersion();

            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-v", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***{Constants.version}***");
        }
    }
}
