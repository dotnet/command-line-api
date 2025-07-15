// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class VersionOptionTests
    {
        private static readonly string version = (Assembly.GetEntryAssembly() ??
                                                  Assembly.GetExecutingAssembly())
                                                 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                 .InformationalVersion;

        [Fact]
        public async Task When_the_version_option_is_specified_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new RootCommand();

            var output = new StringWriter();

            await rootCommand.Parse("--version").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_then_invocation_is_short_circuited()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();
            rootCommand.SetAction(_ => wasCalled = true);

            var output = new StringWriter();

            await rootCommand.Parse("--version").InvokeAsync(new() { Output = output }, CancellationToken.None);

            wasCalled.Should().BeFalse();
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/2628
        public void When_the_version_option_is_specified_then_there_are_no_parse_errors_due_to_unspecified_subcommand()
        {
            Command subcommand = new("subcommand");
            RootCommand root = new()
            {
                subcommand
            };
            subcommand.SetAction(_ => 0);

            var parseResult = root.Parse("--version");

            parseResult.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            var output = new StringWriter();
            await new RootCommand().Parse("--help").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Match("*Options:*--version*Show version information*");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_options_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new RootCommand
            {
                new Option<bool>("-x")
                {
                    DefaultValueFactory = (_) => true
                },
            };
            rootCommand.SetAction((_) => { });

            var output = new StringWriter();

            await rootCommand.Parse("--version").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_arguments_then_the_version_is_written_to_standard_out()
        {
            RootCommand rootCommand = new()
            {
                new Argument<bool>("x") { DefaultValueFactory = (_) => true },
            };
            rootCommand.SetAction((_) => { });

            var output = new StringWriter();

            await rootCommand.Parse("--version").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Theory]
        [InlineData("--version -x")]
        [InlineData("--version subcommand")]
        public void Version_is_not_valid_with_other_tokens(string commandLine)
        {
            var subcommand = new Command("subcommand");
            subcommand.SetAction(_ => { });
            var rootCommand = new RootCommand
            {
                subcommand,
                new Option<bool>("-x")
            };
            rootCommand.SetAction(_ => { });

            var output = new StringWriter();

            var result = rootCommand.Parse(commandLine);

            result.Errors.Should().Contain(e => e.Message == "--version option cannot be combined with other arguments.");
        }

        [Fact]
        public void Version_option_is_not_added_to_subcommands()
        {
            var rootCommand = new RootCommand
            {
                new Command("subcommand")
            };

            rootCommand
                .Subcommands
                .Single(c => c.Name == "subcommand")
                .Options
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task Version_can_specify_additional_alias()
        {
            RootCommand rootCommand = new();

            rootCommand.Options.Clear();
            rootCommand.Add(new VersionOption("-v", "-version"));

            var output = new StringWriter();

            using var _ = new AssertionScope();

            await rootCommand.Parse("-v").InvokeAsync(new() { Output = output }, CancellationToken.None);
            output.ToString().Should().Be($"{version}{NewLine}");

            output = new StringWriter();
            await rootCommand.Parse("-version").InvokeAsync(new() { Output = output }, CancellationToken.None);
            output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public void Version_is_not_valid_with_other_tokens_when_it_uses_custom_alias()
        {
            var childCommand = new Command("subcommand");
            childCommand.SetAction(_ => { });
            var rootCommand = new RootCommand
            {
                childCommand
            };

            rootCommand.Options.Clear();
            rootCommand.Add(new VersionOption("-v"));

            rootCommand.SetAction(_ => { });

            var result = rootCommand.Parse("-v subcommand");

            result.Errors.Should().ContainSingle(e => e.Message == "-v option cannot be combined with other arguments.");
        }
    }
}