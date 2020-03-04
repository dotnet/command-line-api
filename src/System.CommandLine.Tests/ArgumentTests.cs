// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ArgumentTests : SymbolTests
    {
        [Fact]
        public void By_default_there_is_no_default_value()
        {
            var argument = new Argument();

            argument.HasDefaultValue.Should().BeFalse();
        }

        [Fact]
        public void When_default_value_is_set_to_null_then_HasDefaultValue_is_true()
        {
            var argument = new Argument();

            argument.SetDefaultValue(null);

            argument.HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
        {
            var argument = new Argument();

            argument.SetDefaultValueFactory(() => null);

            argument.HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void When_there_is_no_default_value_then_GetDefaultValue_throws()
        {
            var argument = new Argument<string>("the-arg");

            argument.Invoking(a => a.GetDefaultValue())
                    .Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Argument \"the-arg\" does not have a default value");
        }

        public class CustomParsing
        {
            [Fact]
            public void HasDefaultValue_can_be_set_to_true()
            {
                var argument = new Argument<FileSystemInfo>(result => null, true);

                argument.HasDefaultValue
                        .Should()
                        .BeTrue();
            }

            [Fact]
            public void HasDefaultValue_can_be_set_to_false()
            {
                var argument = new Argument<FileSystemInfo>(result => null, false);

                argument.HasDefaultValue
                        .Should()
                        .BeFalse();
            }

            [Fact]
            public void GetDefaultValue_returns_specified_value()
            {
                var argument = new Argument<string>(result => "the-default", isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .Be("the-default");
            }

            [Fact]
            public void GetDefaultValue_returns_null_when_parse_delegate_returns_true_without_setting_a_value()
            {
                var argument = new Argument<string>(result => null, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void GetDefaultValue_returns_null_when_parse_delegate_returns_true_and_sets_value_to_null()
            {
                var argument = new Argument<string>(result => null, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void GetDefaultValue_can_return_null()
            {
                var argument = new Argument<string>(result => null, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void Validation_failure_message_can_be_specified_when_parsing_tokens()
            {
                var argument = new Argument<FileSystemInfo>(result =>
                {
                    result.ErrorMessage = "oops!";
                    return null;
                });

                argument.Parse("x")
                        .Errors
                        .Should()
                        .ContainSingle(e => e.SymbolResult.Symbol == argument)
                        .Which
                        .Message
                        .Should()
                        .Be("oops!");
            }

            [Fact]
            public void Validation_failure_message_can_be_specified_when_evaluating_default_argument_value()
            {
                var argument = new Argument<FileSystemInfo>(result =>
                {
                    result.ErrorMessage = "oops!";
                    return null;
                }, true);

                argument.Parse("")
                        .Errors
                        .Should()
                        .ContainSingle(e => e.SymbolResult.Symbol == argument)
                        .Which
                        .Message
                        .Should()
                        .Be("oops!");
            }

            [Fact]
            public void Validation_failure_message_can_be_specified_when_evaluating_default_option_value()
            {
                var option = new Option<FileSystemInfo>(
                    "-x",
                    result =>
                    {
                        result.ErrorMessage = "oops!";
                        return null;
                    }, true);

                option.Parse("")
                      .Errors
                      .Should()
                      .ContainSingle()
                      .Which
                      .Message
                      .Should()
                      .Be("oops!");
            }

            [Fact]
            public void custom_parsing_of_scalar_value_from_an_argument_with_one_token()
            {
                var argument = new Argument<int>(result => int.Parse(result.Tokens.Single().Value));

                argument.Parse("123")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .Be(123);
            }

            [Fact]
            public void custom_parsing_of_sequence_value_from_an_argument_with_one_token()
            {
                var argument = new Argument<IEnumerable<int>>(result => result.Tokens.Single().Value.Split(',').Select(int.Parse));

                argument.Parse("1,2,3")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .BeEquivalentTo(new[] { 1, 2, 3 });
            }

            [Fact]
            public void custom_parsing_of_sequence_value_from_an_argument_with_multiple_tokens()
            {
                var argument = new Argument<IEnumerable<int>>(result =>
                {
                    return result.Tokens.Select(t => int.Parse(t.Value)).ToArray();
                });

                argument.Parse("1 2 3")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .BeEquivalentTo(new[] { 1, 2, 3 });
            }

            [Fact]
            public void custom_parsing_of_scalar_value_from_an_argument_with_multiple_tokens()
            {
                var argument = new Argument<int>(result => result.Tokens.Select(t => int.Parse(t.Value)).Sum())
                {
                    Arity = ArgumentArity.ZeroOrMore
                };

                argument.Parse("1 2 3")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .Be(6);
            }

            [Fact]
            public void Option_ArgumentResult_Parent_is_set_correctly_when_token_is_implicit()
            {
                ArgumentResult argumentResult = null;

                var command = new Command("the-command")
                {
                    new Option<string>(
                        "-x",
                        parseArgument: argResult =>
                        {
                            argumentResult = argResult;
                            return null;
                        }, isDefault: true)
                };

                command.Parse("");

                argumentResult
                    .Parent
                    .Symbol
                    .Should()
                    .Be(command.Options.Single());
            }

            [Fact]
            public void Option_ArgumentResult_parentage_to_root_symbol_is_set_correctly_when_token_is_implicit()
            {
                ArgumentResult argumentResult = null;

                var command = new Command("the-command")
                {
                    new Option<string>(
                        "-x",
                        parseArgument: argResult =>
                        {
                            argumentResult = argResult;
                            return null;
                        }, isDefault: true)
                };

                command.Parse("");

                argumentResult
                    .Parent
                    .Parent
                    .Symbol
                    .Should()
                    .Be(command);
            }

            [Fact]
            public void Command_ArgumentResult_Parent_is_set_correctly_when_token_is_implicit()
            {
                ArgumentResult argumentResult = null;

                var command = new Command("the-command")
                {
                    new Argument<string>(
                        parse: argResult =>
                        {
                            argumentResult = argResult;
                            return null;
                        }, isDefault: true)
                };

                command.Parse("");

                argumentResult
                    .Parent
                    .Symbol
                    .Should()
                    .Be(command);
            }

            [Fact]
            public async Task Custom_argument_parser_is_only_called_once()
            {
                var callCount = 0;
                var handlerWasCalled = false;

                var command = new RootCommand
                {
                    Handler = CommandHandler.Create<int>(Run)
                };
                command.AddOption(new Option("--value")
                {
                    Argument = new Argument<int>(result =>
                    {
                        callCount++;
                        return int.Parse(result.Tokens.Single().Value);
                    })
                });

                await command.InvokeAsync("--value 42");

                callCount.Should().Be(1);
                handlerWasCalled.Should().BeTrue();

                void Run(int value) => handlerWasCalled = true;
            }

            [Fact]
            public void Default_value_and_custom_argument_parser_can_be_used_together()
            {
                var argument = new Argument<int>(_ => 789, true);
                argument.SetDefaultValue(123);

                var result =  argument.Parse("");

                var argumentResult = result.FindResultFor(argument);

                argumentResult
                    .GetValueOrDefault<int>()
                    .Should()
                    .Be(123);
            }
            
            [Fact]
            public void Multiple_command_arguments_can_have_custom_parse_delegates()
            {
                var root = new RootCommand
                {
                    new Argument<FileInfo[]>("from", argumentResult =>
                    {
                        argumentResult.ErrorMessage = "nope";
                        return null;
                    }, true)
                    {
                        Arity = new ArgumentArity(0, 2)
                    },
                    new Argument<DirectoryInfo>("to", argumentResult =>
                    {
                        argumentResult.ErrorMessage = "UH UH";
                        return null;
                    }, true)
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                };

                var result = root.Parse("a.txt b.txt /path/to/dir");

                result.Errors
                      .Select(e => e.Message)
                      .Should()
                      .Contain("nope");

                result.Errors
                      .Select(e => e.Message)
                      .Should()
                      .Contain("UH UH");
            }

            [Fact]
            public void When_custom_conversion_fails_then_an_option_does_not_accept_further_arguments()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>(),
                    new Option("-x")
                    {
                        Argument = new Argument<string>(argResult =>
                        {
                            argResult.ErrorMessage = "nope";
                            return default;
                        })
                    }
                };

                var result = command.Parse("the-command -x nope yep");

                result.CommandResult.Tokens.Count.Should().Be(1);
            }
            
            [Fact]
            public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
            {
                var command = new Command("the-command")
                {
                    new Option(new[] { "-o", "--one" })
                    {
                        Argument = new Argument<int>(argumentResult =>
                        {
                            if (int.TryParse(argumentResult.Tokens.Select(t => t.Value).Single(), out var value))
                            {
                                return value;
                            }

                            argumentResult.ErrorMessage = $"'{argumentResult.Tokens.Single().Value}' is not an integer";

                            return default;
                        }),
                        Description = ""
                    }
                };

                var result = command.Parse("the-command -o not-an-int");

                Action getValue = () =>
                    result.CommandResult.ValueForOption("o");

                getValue.Should()
                        .Throw<InvalidOperationException>()
                        .Which
                        .Message
                        .Should()
                        .Be("'not-an-int' is not an integer");
            }

        }

        protected override Symbol CreateSymbol(string name)
        {
            return new Argument(name);
        }
    }
}