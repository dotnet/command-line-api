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
        [Fact]
        public async Task Suggest_directive_writes_suggestions_for_option_arguments()
        {
            var eatCommand = new Command("eat");
            var fruitOption = new Option("--fruit",
                                         argument: new Argument<string>().WithSuggestions("apple", "banana", "cherry"));
            eatCommand.AddOption(fruitOption);

            var parser = new CommandLineBuilder()
                         .AddCommand(eatCommand)
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
        public async Task Suggest_directive_writes_suggestions_for_options()
        {
            var fruitOption = new Option("--fruit",
                                         argument: new Argument<string>().WithSuggestions("apple", "banana", "cherry"));

            var parser = new CommandLineBuilder()
                         .AddOption("--vegetable", "A fruit")
                         .AddOption(fruitOption)
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
    }
}
