// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Tests
{
    public class HelpSubsystemTests
    {
        [Fact]
        public void When_help_subsystem_is_used_the_help_option_is_added_to_each_command_in_the_tree()
        {
            var rootCommand = new CliRootCommand
                {
                    new CliOption<bool>("-x"), // add option that is expected for the test data used here
                    new CliCommand("a")
                };
            var configuration = new CliConfiguration(rootCommand);

            var pipeline = Pipeline.CreateEmpty();
            pipeline.Help = new HelpSubsystem();

            // Parse is used because directly calling Initialize would be unusual
            var result = pipeline.Parse(configuration, "");

            rootCommand.Options
                .Count(x => x.Name == "--help")
                .Should()
                .Be(1);
            var subcommand = rootCommand.Subcommands.First();
            subcommand.Options.Should().NotBeNull();
            subcommand.Options
                .Count(x => x.Name == "--help")
                .Should()
                .Be(1);
        }

        [Theory]
        [ClassData(typeof(TestData.Help))]
        public void Help_is_activated_only_when_requested(string input, bool result)
        {
            CliRootCommand rootCommand = [new CliOption<bool>("-x")]; // add random option as empty CLIs are rare
            var configuration = new CliConfiguration(rootCommand);
            var helpSubsystem = new HelpSubsystem();
            var args = CliParser.SplitCommandLine(input).ToList().AsReadOnly();

            Subsystem.Initialize(helpSubsystem, configuration, args);

            var parseResult = CliParser.Parse(rootCommand, input, configuration);
            var isActive = Subsystem.GetIsActivated(helpSubsystem, parseResult);

            isActive.Should().Be(result);
        }

        [Fact]
        public void Outputs_help_message()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var helpSubsystem = new HelpSubsystem();
            Subsystem.Execute(helpSubsystem, new PipelineResult(null, "", null, consoleHack));
            consoleHack.GetBuffer().Trim().Should().Be("Help me!");
        }

        [Fact]
        public void Custom_help_subsystem_can_be_used()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = Pipeline.CreateEmpty();
            pipeline.Help = new AlternateSubsystems.AlternateHelp();

            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-h", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***Help me!***");
        }

        [Fact]
        public void Custom_help_subsystem_can_replace_standard()
        {
            var consoleHack = new ConsoleHack().RedirectToBuffer(true);
            var pipeline = Pipeline.CreateEmpty();
            pipeline.Help = new AlternateSubsystems.AlternateHelp();

            pipeline.Execute(new CliConfiguration(new CliRootCommand()), "-h", consoleHack);
            consoleHack.GetBuffer().Trim().Should().Be($"***Help me!***");
        }
    }
}