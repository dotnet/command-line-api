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
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse("[suggest] e");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"eat{NewLine}");
        }
    }
}
