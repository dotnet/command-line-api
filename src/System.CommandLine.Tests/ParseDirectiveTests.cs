// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParseDirectiveTests
    {
        private readonly ITestOutputHelper output;

        public ParseDirectiveTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Parse_directive_writes_parse_diagram()
        {
            var rootCommand = new RootCommand();
            var subcommand = new Command("subcommand");
            rootCommand.Subcommands.Add(subcommand);
            var option = new Option<int>("-c", "--count");
            subcommand.Options.Add(option);

            var config = new CommandLineBuilder(rootCommand)
                         .UseParseDirective()
                         .Build();
            config.Output = new StringWriter();

            var result = rootCommand.Parse("[parse] subcommand -c 34 --nonexistent wat", config);

            output.WriteLine(result.Diagram());

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"![ {RootCommand.ExecutableName} [ subcommand [ -c <34> ] ] ]   ???--> --nonexistent wat" + Environment.NewLine);
        }

        [Fact]
        public async Task When_parse_directive_is_used_the_help_is_not_displayed()
        {
            var rootCommand = new RootCommand();

            var config = new CommandLineBuilder(rootCommand)
                         .UseParseDirective()
                         .UseVersionOption()
                         .UseHelp()
                         .Build();
            config.Output = new StringWriter();
            var result = rootCommand.Parse("[parse] --help", config);

            output.WriteLine(result.Diagram());

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"[ {RootCommand.ExecutableName} [ --help ] ]" + Environment.NewLine);
        }

        [Fact]
        public async Task When_parse_directive_is_used_the_version_is_not_displayed()
        {
            var rootCommand = new RootCommand();

            var config = new CommandLineBuilder(rootCommand)
                         .UseParseDirective()
                         .UseVersionOption()
                         .UseHelp()
                         .Build();
            config.Output = new StringWriter();

            var result = rootCommand.Parse("[parse] --version", config);

            output.WriteLine(result.Diagram());

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"[ {RootCommand.ExecutableName} [ --version ] ]" + Environment.NewLine);
        }

        [Fact]
        public async Task When_there_are_no_errors_then_parse_directive_sets_exit_code_0()
        {
            var command = new RootCommand
            {
                new Option<int>("-x")
            };

            var exitCode = await command.Parse("[parse] -x 123").InvokeAsync();

            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task When_there_are_errors_then_parse_directive_sets_exit_code_1()
        {
            var command = new RootCommand
            {
                new Option<int>("-x")
            };

            var exitCode = await command.Parse("[parse] -x not-an-int").InvokeAsync();

            exitCode.Should().Be(1);
        }

        [Fact]
        public async Task When_there_are_errors_then_parse_directive_sets_exit_code_to_custom_value()
        {
            var command = new RootCommand
            {
                new Option<int>("-x")
            };

            int exitCode = await new CommandLineBuilder(command)
                                 .UseParseDirective(errorExitCode: 42)
                                 .Build()
                                 .InvokeAsync("[parse] -x not-an-int");

            exitCode.Should().Be(42);
        }
    }
}
