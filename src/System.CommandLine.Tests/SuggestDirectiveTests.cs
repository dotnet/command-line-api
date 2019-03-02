﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

            _eatCommand = new Command("eat")
                          {
                              _fruitOption,
                              _vegetableOption
                          };
        }

        public static IEnumerable<object[]> Exes()
        {
            yield return new[] { "" };
            yield return new[] { RootCommand.ExeName };
            yield return new[] { RootCommand.ExePath };
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_option_arguments_when_under_subcommand(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _eatCommand
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseDefaults()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] eat --fruit ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_option_arguments_when_under_root_command(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _fruitOption,
                                  _vegetableOption
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] --fruit ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"apple{NewLine}banana{NewLine}cherry{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_option_aliases_under_subcommand(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _eatCommand
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] eat ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_option_aliases_under_root_command(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _vegetableOption,
                                  _fruitOption
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}--vegetable{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_subcommand_aliases_under_root_command(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _eatCommand
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] ");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"eat{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_partial_option_aliases_under_root_command(string exe)
        {
            var rootCommand = new RootCommand
                              {
                                  _fruitOption,
                                  _vegetableOption
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseSuggestDirective()
                         .Build();

            var result = parser.Parse($"{exe} [suggest] f");

            var console = new TestConsole();

            await parser.InvokeAsync(result, console);

            console.Out
                   .ToString()
                   .Should()
                   .Be($"--fruit{NewLine}");
        }

        [Theory]
        [MemberData(nameof(Exes))]
        public async Task Suggest_directive_writes_suggestions_for_partial_subcommand_aliases_under_root_command(string exe)
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
