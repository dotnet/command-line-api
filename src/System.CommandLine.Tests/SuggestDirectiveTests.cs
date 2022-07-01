// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
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
            _fruitOption = new Option<string>("--fruit")
                .AddCompletions("apple", "banana", "cherry");

            _vegetableOption = new Option<string>("--vegetable")
                .AddCompletions(_ => new[] { "asparagus", "broccoli", "carrot" });

            _eatCommand = new Command("eat")
            {
                _fruitOption,
                _vegetableOption
            };
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_subcommand()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _eatCommand
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"[suggest:13] \"eat --fruit\"");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_option_arguments_when_under_root_command()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _fruitOption,
                             _vegetableOption
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"[suggest:8] \"--fruit\"");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Theory]
        [InlineData("[suggest:4] \"eat\"")]
        [InlineData("[suggest:6] \"eat --\"")]
        public async Task It_writes_suggestions_for_option_aliases_under_subcommand(string commandLine)
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _eatCommand
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse(commandLine);

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Theory]
        [InlineData("[suggest]")]
        [InlineData("[suggest:0]")]
        [InlineData("[suggest] ")]
        [InlineData("[suggest:0] ")]
        public async Task It_writes_suggestions_for_option_aliases_under_root_command(string input)
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _vegetableOption,
                             _fruitOption
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse(input);

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _eatCommand
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest]");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"eat{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _fruitOption,
                             _vegetableOption
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"[suggest:1] \"f\"");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             _eatCommand,
                             new Command("wash-dishes")
                         })
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest:1] \"d\"");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"wash-dishes{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder(new RootCommand
                          {
                              _eatCommand,
                              new Command("wash-dishes")
                          })
                          .UseSuggestDirective()
                          .UseVersionOption()
                          .Build();

            var result = parser.Parse("[suggest:5] \"--ver\"");

            var console = new TestConsole();

            await result.InvokeAsync(console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--version{NewLine}");
        }

        [Fact]
        public async Task It_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command_with_an_argument()
        {
            var parser = new CommandLineBuilder(new Command("parent")
                          {
                              new Command("child"),
                              new Option<bool>("--option1"),
                              new Option<bool>("--option2"),
                              new Argument<string>()
                          })
                          .UseSuggestDirective()
                          .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("[suggest:3] \"opt\"", console);

            console.Out
                   .ToString()
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

            var console = new TestConsole();

            var commandLine = "--bool-option false";

            await command.InvokeAsync($"[suggest:{commandLine.Length + 1}] \"{commandLine}\"", console);

            console.Out
                   .ToString()
                   .Should()
                   .NotContain("--bool-option");
        }
    }
}
