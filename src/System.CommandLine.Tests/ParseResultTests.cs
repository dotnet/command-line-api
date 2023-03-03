// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ParseResultTests
    {
        [Fact]
        public void An_option_with_a_default_value_and_no_explicitly_provided_argument_has_an_empty_arguments_property()
        {
            var option = new Option<string>("-x") { DefaultValueFactory = (_) => "default" };

            var result = new RootCommand
            {
                option
            }.Parse("-x")
             .FindResultFor(option);

            result.Tokens.Should().BeEmpty();
        }

        [Fact]
        public void FindResult_can_be_used_to_check_the_presence_of_an_option()
        {
            var option = new Option<bool>("-h", "--help");

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -h");

            result.FindResultFor(option).Should().NotBeNull();
        }

        [Fact]
        public void FindResultFor_can_be_used_to_check_the_presence_of_an_implicit_option()
        {
            var option = new Option<int>("-c", "--count") { DefaultValueFactory = (_) => 5 };
            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command");

            result.FindResultFor(option).Should().NotBeNull();
        }

        [Fact]
        public void Command_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var command = new Command("outer")
            {
                new Command("inner-one")
                {
                    new Argument<bool>("arg1")
                    {
                        Arity = ArgumentArity.Zero
                    }
                },
                new Command("inner-two")
                {
                    new Argument<bool>("arg2")
                    {
                        Arity = ArgumentArity.Zero
                    }
                }
            };

            var result = Parser.Parse(command, "outer inner-one inner-two");

            result.CommandResult.Command.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = Parser.Parse(command, "outer inner-two inner-one");

            result2.CommandResult.Command.Name.Should().Be("inner-two");
            result2.Errors.Count.Should().Be(1);
        }

        [Fact] // https://github.com/dotnet/command-line-api/pull/2030#issuecomment-1400275332
        public void ParseResult_GetCompletions_returns_global_options_of_given_command_only()
        {
            var leafCommand = new Command("leafCommand")
            {
                new Option<string>("--one") { Description = "option one" },
                new Option<string>("--two") { Description = "option two" }
            };

            var midCommand1 = new Command("midCommand1")
            {
                leafCommand
            };
            midCommand1.Options.Add(new Option<string>("--three1") { Description = "option three 1", AppliesToSelfAndChildren = true });

            var midCommand2 = new Command("midCommand2")
            {
                leafCommand
            };
            midCommand2.Options.Add(new Option<string>("--three2") { Description = "option three 2", AppliesToSelfAndChildren = true });

            var rootCommand = new Command("root")
            {
                midCommand1,
                midCommand2
            };

            var result = Parser.Parse(rootCommand, "root midCommand2 leafCommand --");

            var completions = result.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three2");
        }
    }
}
