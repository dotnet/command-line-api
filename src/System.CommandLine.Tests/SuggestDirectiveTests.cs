// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class SuggestDirectiveTests
    {
        protected CliOption _fruitOption;

        protected CliOption _vegetableOption;

        private readonly CliCommand _eatCommand;

        public SuggestDirectiveTests()
        {
            _fruitOption = new CliOption<string>("--fruit");
            _fruitOption.CompletionSources.Add("apple", "banana", "cherry");

            _vegetableOption = new CliOption<string>("--vegetable");
            _vegetableOption.CompletionSources.Add(_ => new[] { "asparagus", "broccoli", "carrot" });

            _eatCommand = new CliCommand("eat")
            {
                _fruitOption,
                _vegetableOption
            };
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_subcommand()
        {
            CliRootCommand rootCommand = new()
            {
                _eatCommand,
                new SuggestDirective()
            };
            CliConfiguration config = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("[suggest:13] \"eat --fruit\"", config);

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_root_command()
        {
            CliRootCommand rootCommand = new ()
            {
                _fruitOption,
                _vegetableOption
            };
            CliConfiguration config = new (rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse($"[suggest:8] \"--fruit\"", config);

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Theory]
        [InlineData("[suggest:4] \"eat\"", new[] { "--fruit", "--help", "--vegetable", "-?", "-h", "/?", "/h" })]
        [InlineData("[suggest:6] \"eat --\"", new[] { "--fruit", "--help", "--vegetable" })]
        public async Task It_writes_suggestions_for_option_aliases_under_subcommand(string commandLine, string[] expectedCompletions)
        {
            CliRootCommand rootCommand = new() { _eatCommand };
            CliConfiguration config = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse(commandLine, config);

            await result.InvokeAsync();

            string expected = string.Join(NewLine, expectedCompletions) + NewLine;

            config.Output
                   .ToString()
                   .Should()
                   .Be(expected);
        }

        [Theory]
        [InlineData("[suggest]")]
        [InlineData("[suggest:0]")]
        [InlineData("[suggest] ")]
        [InlineData("[suggest:0] ")]
        public async Task It_writes_suggestions_for_option_aliases_under_root_command(string input)
        {
            CliRootCommand rootCommand = new()
            {
                _vegetableOption,
                _fruitOption
            };
            CliConfiguration config = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse(input, config);
            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--help{NewLine}--vegetable{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_subcommand_aliases_under_root_command()
        {
            CliRootCommand rootCommand = new()
            {
                _eatCommand
            };
            CliConfiguration config = new(rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("[suggest]", config);
            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"--help{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}eat{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_aliases_under_root_command()
        {
            CliRootCommand rootCommand = new()
            {
                _fruitOption,
                _vegetableOption
            };
            CliConfiguration config = new (rootCommand)
            {
                Output = new StringWriter(),
            };

            var result = rootCommand.Parse("[suggest:1] \"f\"", config);

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_subcommand_aliases_under_root_command()
        {
            CliRootCommand rootCommand = new ()
            {
                _eatCommand,
                new CliCommand("wash-dishes")
            };
            CliConfiguration config = new (rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("[suggest:1] \"d\"", config);

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"wash-dishes{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command()
        {
            CliRootCommand rootCommand = new ()
            {
                _eatCommand,
                new CliCommand("wash-dishes"),
            };
            CliConfiguration config = new (rootCommand)
            {
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("[suggest:5] \"--ver\"", config);

            await result.InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .Be($"--version{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command_with_an_argument()
        {
            CliRootCommand command = new("parent")
            {
                new CliCommand("child"),
                new CliOption<bool>("--option1"),
                new CliOption<bool>("--option2"),
                new CliArgument<string>("arg")
            };
            CliConfiguration config = new (command)
            {
                Output = new StringWriter()
            };

            await config.InvokeAsync("[suggest:3] \"opt\"");

            config.Output
                   .ToString()
                   .Should()
                   .Be($"--option1{NewLine}--option2{NewLine}");
        }

        [Fact]
        public async Task It_does_not_repeat_suggestion_for_already_specified_bool_option()
        {
            var command = new CliRootCommand
            {
                new CliOption<bool>("--bool-option")
            };
            CliConfiguration config = new (command)
            {
                Output = new StringWriter()
            };

            var commandLine = "--bool-option false";

            await command.Parse($"[suggest:{commandLine.Length + 1}] \"{commandLine}\"", config).InvokeAsync();

            config.Output
                   .ToString()
                   .Should()
                   .NotContain("--bool-option");
        }
    }
}
