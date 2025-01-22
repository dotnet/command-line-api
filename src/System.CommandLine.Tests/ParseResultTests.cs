// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
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
             .GetResult(option);

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

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void GetResult_can_be_used_to_check_the_presence_of_an_implicit_option()
        {
            var option = new Option<int>("-c", "--count") { DefaultValueFactory = (_) => 5 };
            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command");

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void GetResult_can_be_used_for_root_command_itself()
        {
            RootCommand rootCommand = new()
            {
                new Command("the-command")
                {
                    new Option<int>("-c")
                }
            };

            var result = rootCommand.Parse("the-command -c 123");

            result.RootCommandResult.Command.Should().BeSameAs(rootCommand);
            result.GetResult(rootCommand).Should().BeSameAs(result.RootCommandResult);
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

            var result = CommandLineParser.Parse(command, "outer inner-one inner-two");

            result.CommandResult.Command.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = CommandLineParser.Parse(command, "outer inner-two inner-one");

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
            midCommand1.Options.Add(new Option<string>("--three1") { Description = "option three 1", Recursive = true });

            var midCommand2 = new Command("midCommand2")
            {
                leafCommand
            };
            midCommand2.Options.Add(new Option<string>("--three2") { Description = "option three 2", Recursive = true });

            var rootCommand = new Command("root")
            {
                midCommand1,
                midCommand2
            };

            var result = CommandLineParser.Parse(rootCommand, "root midCommand2 leafCommand --");

            var completions = result.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three2");
        }

        [Fact]
        public void Handler_is_null_when_parsed_command_did_not_specify_handler()
            => new RootCommand().Parse("").Action.Should().BeNull();

        [Fact]
        public void Handler_is_not_null_when_parsed_command_specified_handler()
        {
            bool handlerWasCalled = false;

            RootCommand command = new();
            command.SetAction((_) => handlerWasCalled = true);

            ParseResult parseResult = command.Parse("");

            parseResult.Action.Should().NotBeNull();
            handlerWasCalled.Should().BeFalse();

            ((SynchronousCommandLineAction)parseResult.Action!).Invoke(null!).Should().Be(0);
            handlerWasCalled.Should().BeTrue();
        }
    }
}
