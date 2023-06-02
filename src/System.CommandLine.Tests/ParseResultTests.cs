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
            var option = new CliOption<string>("-x") { DefaultValueFactory = (_) => "default" };

            var result = new CliRootCommand
            {
                option
            }.Parse("-x")
             .GetResult(option);

            result.Tokens.Should().BeEmpty();
        }

        [Fact]
        public void FindResult_can_be_used_to_check_the_presence_of_an_option()
        {
            var option = new CliOption<bool>("-h", "--help");

            var command = new CliCommand("the-command")
            {
                option
            };

            var result = command.Parse("the-command -h");

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void GetResult_can_be_used_to_check_the_presence_of_an_implicit_option()
        {
            var option = new CliOption<int>("-c", "--count") { DefaultValueFactory = (_) => 5 };
            var command = new CliCommand("the-command")
            {
                option
            };

            var result = command.Parse("the-command");

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void GetResult_can_be_used_for_root_command_itself()
        {
            CliRootCommand rootCommand = new()
            {
                new CliCommand("the-command")
                {
                    new CliOption<int>("-c")
                }
            };

            var result = rootCommand.Parse("the-command -c 123");

            result.RootCommandResult.Command.Should().BeSameAs(rootCommand);
            result.GetResult(rootCommand).Should().BeSameAs(result.RootCommandResult);
        }

        [Fact]
        public void Command_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var command = new CliCommand("outer")
            {
                new CliCommand("inner-one")
                {
                    new CliArgument<bool>("arg1")
                    {
                        Arity = ArgumentArity.Zero
                    }
                },
                new CliCommand("inner-two")
                {
                    new CliArgument<bool>("arg2")
                    {
                        Arity = ArgumentArity.Zero
                    }
                }
            };

            var result = CliParser.Parse(command, "outer inner-one inner-two");

            result.CommandResult.Command.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = CliParser.Parse(command, "outer inner-two inner-one");

            result2.CommandResult.Command.Name.Should().Be("inner-two");
            result2.Errors.Count.Should().Be(1);
        }

        [Fact] // https://github.com/dotnet/command-line-api/pull/2030#issuecomment-1400275332
        public void ParseResult_GetCompletions_returns_global_options_of_given_command_only()
        {
            var leafCommand = new CliCommand("leafCommand")
            {
                new CliOption<string>("--one") { Description = "option one" },
                new CliOption<string>("--two") { Description = "option two" }
            };

            var midCommand1 = new CliCommand("midCommand1")
            {
                leafCommand
            };
            midCommand1.Options.Add(new CliOption<string>("--three1") { Description = "option three 1", Recursive = true });

            var midCommand2 = new CliCommand("midCommand2")
            {
                leafCommand
            };
            midCommand2.Options.Add(new CliOption<string>("--three2") { Description = "option three 2", Recursive = true });

            var rootCommand = new CliCommand("root")
            {
                midCommand1,
                midCommand2
            };

            var result = CliParser.Parse(rootCommand, "root midCommand2 leafCommand --");

            var completions = result.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three2");
        }

        [Fact]
        public void Handler_is_null_when_parsed_command_did_not_specify_handler()
            => new CliRootCommand().Parse("").Action.Should().BeNull();

        [Fact]
        public void Handler_is_not_null_when_parsed_command_specified_handler()
        {
            bool handlerWasCalled = false;

            CliRootCommand command = new();
            command.SetAction((_) => handlerWasCalled = true);

            ParseResult parseResult = command.Parse("");

            parseResult.Action.Should().NotBeNull();
            handlerWasCalled.Should().BeFalse();

            parseResult.Action.Invoke(null!).Should().Be(0);
            handlerWasCalled.Should().BeTrue();
        }
    }
}
