// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using Xunit;
using System.CommandLine.Parsing;

namespace System.CommandLine.Extended.Tests
{
    public class VersionOptionTests
    {
// TODO: invocation/output
/*
        private static readonly string version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                 .InformationalVersion;

        [Fact]
        public async Task When_the_version_option_is_specified_then_the_version_is_written_to_standard_out()
        {
            CliConfiguration configuration = new(new CliRootCommand())
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_then_invocation_is_short_circuited()
        {
            var wasCalled = false;
            var rootCommand = new CliRootCommand();
            rootCommand.SetAction((_) => wasCalled = true);

            CliConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_the_version_option_is_specified_then_the_version_is_parsed()
        {
            ParseResult parseResult = CliParser.Parse (
                new CliRootCommand(),
                [ "--version"]);

            parseResult.Errors.Should().BeEmpty();
            parseResult.GetValue(configuration.RootCommand.Options.OfType<VersionOption>().Single()).Should().BeTrue();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            CliConfiguration configuration = new(new CliRootCommand())
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--help");

            configuration.Output
                   .ToString()
                   .Should()
                   .Match("*Options:*--version*Show version information*");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_options_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new CliRootCommand
            {
                new CliOption<bool>("-x")
                {
                    DefaultValueFactory = (_) => true
                },
            };
            rootCommand.SetAction((_) => { });

            CliConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_arguments_then_the_version_is_written_to_standard_out()
        {
            CliRootCommand rootCommand = new()
            {
                new CliArgument<bool>("x") { DefaultValueFactory =(_) => true },
            };
            rootCommand.SetAction((_) => { });

            CliConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
        }
*/

        const string SkipValidationTests = "VersionOption does not yet do validation";

        [Theory]
        [InlineData("--version", "-x", Skip = SkipValidationTests)]
        [InlineData("--version", "subcommand", Skip = SkipValidationTests)]
        public void Version_is_not_valid_with_other_tokens(params string[] commandLine)
        {
            var subcommand = new CliCommand("subcommand");
            var rootCommand = new CliRootCommand
            {
                subcommand,
                new CliOption<bool>("-x")
            };

            var result = CliParser.Parse(rootCommand, commandLine);

            result.Errors.Should().Contain(e => e.Message == "--version option cannot be combined with other arguments.");
        }
        
        [Fact]
        public void Version_option_is_not_added_to_subcommands()
        {
            var childCommand = new CliCommand("subcommand");

            var rootCommand = new CliRootCommand
            {
                childCommand
            };

            rootCommand
                .Subcommands
                .Single(c => c.Name == "subcommand")
                .Options
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Version_can_specify_additional_alias()
        {
            var versionOption = new VersionOption("-version", "-v");
            CliRootCommand rootCommand = [versionOption];

            var parseResult = CliParser.Parse(rootCommand, ["-version"]);
            var versionSpecified = parseResult.GetValue(versionOption);
            versionSpecified.Should().BeTrue();

            parseResult = CliParser.Parse(rootCommand, ["-v"]);
            versionSpecified = parseResult.GetValue(versionOption);
            versionSpecified.Should().BeTrue();
        }

        [Fact(Skip = SkipValidationTests)]
        public void Version_is_not_valid_with_other_tokens_uses_custom_alias()
        {
            var childCommand = new CliCommand("subcommand");
            var rootCommand = new CliRootCommand
            {
                childCommand
            };

            rootCommand.Options[0] = new VersionOption("-v");

            var result = CliParser.Parse(rootCommand, ["-v", "subcommand"]);

            result.Errors.Should().ContainSingle(e => e.Message == "-v option cannot be combined with other arguments.");
        }
    }
}