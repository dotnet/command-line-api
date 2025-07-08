// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class SuggestDirectiveTests
    {
        protected Option _fruitOption;

        protected Option _vegetableOption;

        private readonly Command _eatCommand;

        public SuggestDirectiveTests()
        {
            _fruitOption = new Option<string>("--fruit");
            _fruitOption.CompletionSources.Add("apple", "banana", "cherry");

            _vegetableOption = new Option<string>("--vegetable");
            _vegetableOption.CompletionSources.Add(_ => new[] { "asparagus", "broccoli", "carrot" });

            _eatCommand = new Command("eat")
            {
                _fruitOption,
                _vegetableOption
            };
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_subcommand()
        {
            RootCommand rootCommand = new()
            {
                _eatCommand,
                new SuggestDirective()
            };
            var output= new StringWriter();

            var result = rootCommand.Parse("[suggest:13] \"eat --fruit\"");

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_root_command()
        {
            RootCommand rootCommand = new()
            {
                _fruitOption,
                _vegetableOption
            };
            var output = new StringWriter();

            var result = rootCommand.Parse($"[suggest:8] \"--fruit\"");

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Theory]
        [InlineData("[suggest:4] \"eat\"", new[] { "--fruit", "--help", "--vegetable", "-?", "-h", "/?", "/h" })]
        [InlineData("[suggest:6] \"eat --\"", new[] { "--fruit", "--help", "--vegetable" })]
        public async Task It_writes_suggestions_for_option_aliases_under_subcommand(string commandLine, string[] expectedCompletions)
        {
            RootCommand rootCommand = new() { _eatCommand };
            
            var output = new StringWriter();

            var result = rootCommand.Parse(commandLine);

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            string expected = string.Join(NewLine, expectedCompletions) + NewLine;

            output.ToString()
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
            RootCommand rootCommand = new()
            {
                _vegetableOption,
                _fruitOption
            };
            var output = new StringWriter();

            await rootCommand.Parse(input).InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"--fruit{NewLine}--help{NewLine}--vegetable{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_subcommand_aliases_under_root_command()
        {
            RootCommand rootCommand = new()
            {
                _eatCommand
            };
            var output = new StringWriter();

            var result = rootCommand.Parse("[suggest]");
            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"--help{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}eat{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_aliases_under_root_command()
        {
            RootCommand rootCommand = new()
            {
                _fruitOption,
                _vegetableOption
            };
            
             var output = new StringWriter();

            var result = rootCommand.Parse("[suggest:1] \"f\"");

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"--fruit{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_subcommand_aliases_under_root_command()
        {
            RootCommand rootCommand = new ()
            {
                _eatCommand,
                new Command("wash-dishes")
            };
            var output = new StringWriter();

            var result = rootCommand.Parse("[suggest:1] \"d\"");

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"wash-dishes{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command()
        {
            RootCommand rootCommand = new ()
            {
                _eatCommand,
                new Command("wash-dishes"),
            };
            var output = new StringWriter();

            var result = rootCommand.Parse("[suggest:5] \"--ver\"");

            await result.InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .Be($"--version{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command_with_an_argument()
        {
            RootCommand command = new("parent")
            {
                new Command("child"),
                new Option<bool>("--option1"),
                new Option<bool>("--option2"),
                new Argument<string>("arg")
            };

            var output = new StringWriter();

            await command.Parse("[suggest:3] \"opt\"").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output.ToString()
                  .Should()
                  .Be($"--option1{NewLine}--option2{NewLine}");
        }

        [Fact]
        public async Task It_does_not_repeat_suggestion_for_already_specified_bool_option()
        {
            var command = new RootCommand
            {
                new Option<bool>("--bool-option")
            };

            var output = new StringWriter();

            var commandLine = "--bool-option false";

            await command.Parse($"[suggest:{commandLine.Length + 1}] \"{commandLine}\"").InvokeAsync(new() { Output = output }, CancellationToken.None);

            output
                .ToString()
                .Should()
                .NotContain("--bool-option");
        }
    }
}
