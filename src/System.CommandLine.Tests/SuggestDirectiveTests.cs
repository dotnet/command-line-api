// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
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
            _fruitOption = new Option("--fruit",
                                      argument: new Argument<string>()
                                          .WithSuggestions("apple", "banana", "cherry"));

            _vegetableOption = new Option("--vegetable",
                                          argument: new Argument<string>()
                                              .WithSuggestions("asparagus", "broccoli", "carrot"));
            _eatCommand = new Command("eat",
                                      symbols: new[] { _fruitOption, _vegetableOption });
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_option_arguments_when_under_subcommand()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(_eatCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] eat --fruit ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_option_arguments_when_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(_fruitOption)
                         .AddOption(_vegetableOption)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] --fruit ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_option_aliases_under_subcommand()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(_eatCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] eat ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_option_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(_vegetableOption)
                         .AddOption(_fruitOption)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(_eatCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"eat{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_partial_option_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(_vegetableOption)
                         .AddOption(_fruitOption)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] f");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_partial_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(_eatCommand)
                         .AddCommand(new Command("wash-dishes"))
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] d");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"wash-dishes{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(_eatCommand)
                         .AddCommand(new Command("wash-dishes"))
                         .UseDefaults()
                         .Build();

            var result = parser.Parse("[suggest] --ver");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--version{NewLine}");
        }

        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_partial_option_and_subcommand_aliases_under_root_command_with_an_argument()
        {
            var command = new Command("parent")
                          {
                              new Command("child"),
                              new Option("--option1"),
                              new Option("--option2"),
                          };
            command.Argument = new Argument<string>();

            var console = new TestConsole();

            await command.InvokeAsync("[suggest] opt", console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--option1{NewLine}--option2{NewLine}");
        }
    }
}
