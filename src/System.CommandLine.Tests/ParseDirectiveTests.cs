﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
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
            rootCommand.AddCommand(subcommand);
            var option = new Option(new[] { "-c", "--count" })
            {
                Argument = new Argument<int>()
            };
            subcommand.AddOption(option);

            var parser = new CommandLineBuilder(rootCommand)
                         .UseParseDirective()
                         .Build();

            var result = parser.Parse("[parse] subcommand -c 34 --nonexistent wat");

            output.WriteLine(result.Diagram());

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"[ {RootCommand.ExecutableName} [ subcommand [ -c <34> ] ] ]   ???--> --nonexistent wat" + Environment.NewLine);
        }

        [Fact]
        public async Task When_there_are_no_errors_then_parse_directive_sets_exit_code_0()
        {
            var command = new RootCommand
            {
                new Option("-x")
                {
                    Argument = new Argument<int>()
                }
            };

            var exitCode = await command.InvokeAsync("[parse] -x 123");

            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task When_there_are_errors_then_parse_directive_sets_exit_code_1()
        {
            var command = new RootCommand
            {
                new Option("-x")
                {
                    Argument = new Argument<int>()
                }
            };

            var exitCode = await command.InvokeAsync("[parse] -x not-an-int");

            exitCode.Should().Be(1);
        }
    }
}
