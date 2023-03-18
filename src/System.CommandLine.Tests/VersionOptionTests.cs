// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class VersionOptionTests
    {
        private static readonly string version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                         .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                         .InformationalVersion;

        [Fact]
        public async Task When_the_version_option_is_specified_then_the_version_is_written_to_standard_out()
        {
            var configuration = new CommandLineBuilder(new RootCommand())
                         .UseVersionOption()
                         .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("--version");

            configuration.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_then_invocation_is_short_circuited()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();
            rootCommand.SetAction((_) => wasCalled = true);

            var config = new CommandLineBuilder(rootCommand)
                         .UseVersionOption()
                         .Build();
            config.Out = new StringWriter();

            await config.InvokeAsync("--version");

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            var configuration = new CommandLineBuilder(new RootCommand())
                         .UseHelp()
                         .UseVersionOption()
                         .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("--help");

            configuration.Out
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
                }
            };
            rootCommand.SetAction((_) => { });

            var configuration = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("--version");

            configuration.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_arguments_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new RootCommand
            {
                new Argument<bool>("x") { DefaultValueFactory =(_) => true }
            };
            rootCommand.SetAction((_) => { });

            var configuration = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("--version");

            configuration.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Theory]
        [InlineData("--version -x")]
        [InlineData("--version subcommand")]
        public void Version_is_not_valid_with_other_tokens(string commandLine)
        {
            var subcommand = new Command("subcommand");
            subcommand.SetAction((_) => { });
            var rootCommand = new RootCommand
            {
                subcommand,
                new Option<bool>("-x")
            };
            rootCommand.SetAction((_) => { });

            var configuration = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();

            var result = rootCommand.Parse(commandLine, configuration);

            result.Errors.Should().Contain(e => e.Message == "--version option cannot be combined with other arguments.");
        }

        [Fact]
        public void Version_option_is_not_added_to_subcommands()
        {
            var childCommand = new Command("subcommand");
            childCommand.SetAction((_) => { });

            var rootCommand = new RootCommand
            {
                childCommand,
            };
            rootCommand.SetAction((_) => { });

            var configuration = new CommandLineBuilder(rootCommand)
                         .UseVersionOption()
                         .Build();

            configuration
                  .RootCommand
                  .Subcommands
                  .Single(c => c.Name == "subcommand")
                  .Options
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public async Task Version_not_added_if_it_exists()
        {
            // Adding an option multiple times can occur two ways in
            // real world scenarios - invocation can be invoked twice
            // or the author may have their own version switch but
            // still want other defaults.
            var configuration = new CommandLineBuilder(new RootCommand())
                         .UseVersionOption()
                         .UseVersionOption()
                         .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("--version");

            configuration.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task Version_can_specify_additional_alias()
        {
            var configuration = new CommandLineBuilder(new RootCommand())
                         .UseVersionOption("-v", "-version")
                         .Build();
            configuration.Out = new StringWriter();

            await configuration.InvokeAsync("-v");
            configuration.Out.ToString().Should().Be($"{version}{NewLine}");

            configuration.Out = new StringWriter();
            await configuration.InvokeAsync("-version");
            configuration.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public void Version_is_not_valid_with_other_tokens_uses_custom_alias()
        {
            var childCommand = new Command("subcommand");
            childCommand.SetAction((_) => { });
            var rootCommand = new RootCommand
            {
                childCommand
            };
            rootCommand.SetAction((_) => { });

            var configuration = new CommandLineBuilder(rootCommand)
                         .UseVersionOption("-v")
                         .Build();

            var result = rootCommand.Parse("-v subcommand", configuration);

            result.Errors.Should().ContainSingle(e => e.Message == "-v option cannot be combined with other arguments.");
        }
    }
}
