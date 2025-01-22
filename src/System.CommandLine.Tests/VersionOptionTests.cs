// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
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
            CommandLineConfiguration configuration = new(new RootCommand())
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
            var rootCommand = new RootCommand();
            rootCommand.SetAction((_) => wasCalled = true);

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            CommandLineConfiguration configuration = new(new RootCommand())
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
            var rootCommand = new RootCommand
            {
                new Option<bool>("-x")
                {
                    DefaultValueFactory = (_) => true
                },
            };
            rootCommand.SetAction((_) => { });

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_arguments_then_the_version_is_written_to_standard_out()
        {
            RootCommand rootCommand = new ()
            {
                new Argument<bool>("x") { DefaultValueFactory =(_) => true },
            };
            rootCommand.SetAction((_) => { });

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("--version");

            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
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

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse(commandLine, configuration);

            result.Errors.Should().Contain(e => e.Message == "--version option cannot be combined with other arguments.");
        }
        
        [Fact]
        public void Version_option_is_not_added_to_subcommands()
        {
            var childCommand = new Command("subcommand");
            childCommand.SetAction(_ => { });

            var rootCommand = new RootCommand
            {
                childCommand
            };
            rootCommand.SetAction(_ => { });

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            configuration
                  .RootCommand
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

            for (int i = 0; i < rootCommand.Options.Count; i++)
            {
                if (rootCommand.Options[i] is VersionOption)
                    rootCommand.Options[i] = new VersionOption("-v", "-version");
            }

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            await configuration.InvokeAsync("-v");
            configuration.Output.ToString().Should().Be($"{version}{NewLine}");

            configuration.Output = new StringWriter();
            await configuration.InvokeAsync("-version");
            configuration.Output.ToString().Should().Be($"{version}{NewLine}");
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

            rootCommand.Options[1] = new VersionOption("-v");

            rootCommand.SetAction((_) => { });

            CommandLineConfiguration configuration = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("-v subcommand", configuration);

            result.Errors.Should().ContainSingle(e => e.Message == "-v option cannot be combined with other arguments.");
        }
    }
}
