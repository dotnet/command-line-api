// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
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
            var argument = new Argument<string>();

            argument.HasDefaultValue.Should().BeFalse();
        }

        [Fact]
        public void When_default_value_is_set_to_null_then_HasDefaultValue_is_true()
        {
            var argument = new Argument<string>();

            argument.SetDefaultValue(null);

            argument.HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
        {
            var argument = new Argument<string[]>();

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
                        .GetValueForArgument(argument)
                        .Should()
                        .Be(123);
            }

            [Fact]
            public void custom_parsing_of_sequence_value_from_an_argument_with_one_token()
            {
                var argument = new Argument<IEnumerable<int>>(result => result.Tokens.Single().Value.Split(',').Select(int.Parse));

                argument.Parse("1,2,3")
                        .GetValueForArgument(argument)
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
                        .GetValueForArgument(argument)
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
                        .GetValueForArgument(argument)
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
            
            [Theory]
            [InlineData("-x value-x -y value-y")]
            [InlineData("-y value-y -x value-x")]
            public void Symbol_can_be_found_without_explicitly_traversing_result_tree(string commandLine)
            {
                SymbolResult resultForOptionX = null;
                var optionX = new Option<string>(
                    "-x",
                    parseArgument: _ => string.Empty);
                
                var optionY = new Option<string>(
                    "-y",
                    parseArgument: argResult =>
                    {
                        resultForOptionX = argResult.FindResultFor(optionX);
                        return string.Empty;
                    });
            
                var command = new Command("the-command")
                {
                    optionX,
                    optionY,
                };
            
                command.Parse(commandLine);

                resultForOptionX
                    .Should()
                    .BeOfType<OptionResult>()
                    .Which
                    .Option
                    .Should()
                    .BeSameAs(optionX);
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

                var option = new Option<int>("--value", result =>
                {
                    callCount++;
                    return int.Parse(result.Tokens.Single().Value);
                });

                var command = new RootCommand();
                command.SetHandler((int value) => handlerWasCalled = true, option);
                command.AddOption(option);

                await command.InvokeAsync("--value 42");

                callCount.Should().Be(1);
                handlerWasCalled.Should().BeTrue();
            }

            [Fact]
            public void Default_value_and_custom_argument_parser_can_be_used_together()
            {
                var argument = new Argument<int>(_ => 789, true);
                argument.SetDefaultValue(123);

                var result = argument.Parse("");

                result.GetValueForArgument(argument)
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
                    new Option<string>("-x", argResult =>
                        {
                            argResult.ErrorMessage = "nope";
                            return default;
                        })
                };

                var result = command.Parse("the-command -x nope yep");

                result.CommandResult.Tokens.Count.Should().Be(1);
            }
            
            [Fact]
            public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
            {
                var option = new Option<int>(new[] { "-o", "--one" }, argumentResult =>
                {
                    if (int.TryParse(argumentResult.Tokens.Select(t => t.Value).Single(), out var value))
                    {
                        return value;
                    }

                    argumentResult.ErrorMessage = $"'{argumentResult.Tokens.Single().Value}' is not an integer";

                    return default;
                });

                var command = new Command("the-command")
                {
                    option
                };

                var result = command.Parse("the-command -o not-an-int");

                Action getValue = () => 
                    result.GetValueForOption(option);

                getValue.Should()
                        .Throw<InvalidOperationException>()
                        .Which
                        .Message
                        .Should()
                        .Be("'not-an-int' is not an integer");
            }

            [Fact]
            public void Parse_delegate_is_called_once_per_parse_operation()
            {
                var i = 0;

                var command = new RootCommand
                {
                    new Option<int>(
                        "-x", 
                        result => ++i, 
                        isDefault: true)
                };

                command.Parse("");
                command.Parse("");

                i.Should().Be(2);
            }

            [Theory] // https://github.com/dotnet/command-line-api/issues/1294
            [InlineData("", "option-is-implicit")]
            [InlineData("--bananas", "argument-is-implicit")]
            [InlineData("--bananas argument-is-specified", "argument-is-specified")]
            public void Parse_delegate_is_called_when_Option_Arity_allows_zero_tokens(string commandLine, string expectedValue)
            {
                var opt = new Option<string>(
                    "--bananas",
                    parseArgument: result =>
                    {
                        if (result.Tokens.Count == 0)
                        {
                            if (result.Parent is OptionResult { IsImplicit: true })
                            {
                                return "option-is-implicit";
                            }

                            return "argument-is-implicit";
                        }
                        else
                        {
                            return result.Tokens[0].Value;
                        }
                    }, isDefault: true)
                {
                    Arity = ArgumentArity.ZeroOrOne
                };

                var rootCommand = new RootCommand
                {
                    opt
                };

                rootCommand.Parse(commandLine).GetValueForOption(opt).Should().Be(expectedValue);
            }

            [Theory]
            [InlineData("1 2 3 4 5 6 7 8")]
            [InlineData("-o 999 1 2 3 4 5 6 7 8")]
            [InlineData("1 2 3 -o 999 4 5 6 7 8")]
            public void Custom_parser_can_pass_on_remaining_tokens(string commandLine)
            {
                var argument1 = new Argument<int[]>(
                    "one",
                    result =>
                    {
                        result.OnlyTake(3);

                        return new[]
                        {
                            int.Parse(result.Tokens[0].Value),
                            int.Parse(result.Tokens[1].Value),
                            int.Parse(result.Tokens[2].Value)
                        };
                    });
                var argument2 = new Argument<int[]>(
                    "two",
                    result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray());
                var command = new RootCommand
                {
                    argument1,
                    argument2,
                    new Option<int>("-o")
                };

                var parseResult = command.Parse(commandLine);

                parseResult.FindResultFor(argument1)
                           .GetValueOrDefault()
                           .Should()
                           .BeEquivalentTo(new[] { 1, 2, 3 },
                                                    options => options.WithStrictOrdering());

                parseResult.FindResultFor(argument2)
                           .GetValueOrDefault()
                           .Should()
                           .BeEquivalentTo(new[] { 4, 5, 6, 7, 8 },
                                                    options => options.WithStrictOrdering());
            }

            [Fact]
            public void When_tokens_are_passed_on_by_custom_parser_on_last_argument_then_they_become_unmatched_tokens()
            {

                var argument1 = new Argument<int[]>(
                    "one",
                    result =>
                    {
                        result.OnlyTake(3);

                        return new[]
                        {
                            int.Parse(result.Tokens[0].Value),
                            int.Parse(result.Tokens[1].Value),
                            int.Parse(result.Tokens[2].Value)
                        };
                    });
             
                var command = new RootCommand
                {
                    argument1
                };

                var parseResult = command.Parse("1 2 3 4 5 6 7 8");

                parseResult.UnmatchedTokens
                           .Should()
                           .BeEquivalentTo(new[] { "4", "5", "6", "7", "8" },
                                           options => options.WithStrictOrdering());
            }

            [Fact]
            public void When_custom_parser_passes_on_tokens_the_argument_result_tokens_reflect_the_change()
            {
                var argument1 = new Argument<int[]>(
                    "one",
                    result =>
                    {
                        result.OnlyTake(3);

                        return new[]
                        {
                            int.Parse(result.Tokens[0].Value),
                            int.Parse(result.Tokens[1].Value),
                            int.Parse(result.Tokens[2].Value)
                        };
                    });
                var argument2 = new Argument<int[]>(
                    "two",
                    result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray());
                var command = new RootCommand
                {
                    argument1,
                    argument2
                };

                var parseResult = command.Parse("1 2 3 4 5 6 7 8");

                parseResult.FindResultFor(argument1)
                           .Tokens
                           .Select(t => t.Value)
                           .Should()
                           .BeEquivalentTo(new[] { "1", "2", "3" },
                                           options => options.WithStrictOrdering());

                parseResult.FindResultFor(argument2)
                           .Tokens
                           .Select(t => t.Value)
                           .Should()
                           .BeEquivalentTo(new[] { "4", "5", "6", "7", "8" },
                                           options => options.WithStrictOrdering());
            }

            [Fact]
            public void OnlyTake_throws_when_called_with_a_negative_value()
            {
                 var argument = new Argument<int[]>(
                    "one",
                    result =>
                    {
                        result.OnlyTake(-1);

                        return null;
                    });

                 argument.Invoking(a => a.Parse("1 2 3"))
                         .Should()
                         .Throw<ArgumentOutOfRangeException>()
                         .Which
                         .Message
                         .Should()
                         .ContainAll("Value must be at least 1.", "Actual value was -1.");
            }

            [Fact]
            public void OnlyTake_throws_when_called_twice()
            {
                 var argument = new Argument<int[]>(
                    "one",
                    result =>
                    {
                        result.OnlyTake(1);
                        result.OnlyTake(1);

                        return null;
                    });

                 argument.Invoking(a => a.Parse("1 2 3"))
                         .Should()
                         .Throw<InvalidOperationException>()
                         .Which
                         .Message
                         .Should()
                         .Be("OnlyTake can only be called once.");
            }

            [Fact]
            public void OnlyTake_can_pass_on_all_tokens_from_one_multiple_arity_argument_to_another()
            {
                var argument1 = new Argument<int[]>(result =>
                {
                    result.OnlyTake(0);
                    return null;
                });
                var argument2 = new Argument<int[]>();
                var command = new RootCommand
                {
                    argument1,
                    argument2
                };

                var result = command.Parse("1 2 3");

                result.GetValueForArgument(argument1).Should().BeEmpty();

                result.GetValueForArgument(argument2).Should().BeEquivalentSequenceTo(1, 2, 3);
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1759 
            public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another()
            {
                var scalar = new Argument<int?>(parse: ctx =>
                {
                    ctx.OnlyTake(0);
                    return null;
                });
                Argument<int[]> multiple = new();

                var command = new RootCommand
                {
                    scalar,
                    multiple
                };

                var result = command.Parse("1 2 3");

                result.GetValueForArgument(scalar).Should().BeNull();

                result.GetValueForArgument(multiple).Should().BeEquivalentSequenceTo(1, 2, 3);
            }


            [Fact] //https://github.com/dotnet/command-line-api/issues/1779
            public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another_that_also_passes_them_all_on()
            {
                var first = new Argument<string>(name: "first", parse: ctx =>
                {
                    ctx.OnlyTake(0);
                    return null;
                })
                {
                    Arity = ArgumentArity.ZeroOrOne
                };

                var second = new Argument<string[]>(name: "second", parse: ctx =>
                {
                    ctx.OnlyTake(0);
                    return null;
                })
                {
                    Arity = ArgumentArity.ZeroOrMore
                };

                var third = new Argument<string[]>(name: "third", parse: ctx =>
                {
                    ctx.OnlyTake(3);
                    return new[] { "1", "2", "3" };
                })
                {
                    Arity = ArgumentArity.ZeroOrMore
                };

                var command = new RootCommand
                {
                    first,
                    second,
                    third
                };

                var result = command.Parse("1 2 3");

                result.GetValueForArgument(first).Should().BeNull();
                result.GetValueForArgument(second).Should().BeEmpty();
                result.GetValueForArgument(third).Should().BeEquivalentSequenceTo("1", "2", "3");
            }
        }

        [Fact]
        public void Argument_of_enum_can_limit_enum_members_as_valid_values()
        {
            var argument = new Argument<ConsoleColor>()
                .FromAmong(ConsoleColor.Red.ToString(), ConsoleColor.Green.ToString());
            Command command = new("set-color")
            {
                argument
            };

            var result = command.Parse("set-color Fuschia");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { $"Argument 'Fuschia' not recognized. Must be one of:\n\t'Red'\n\t'Green'" });
        }

        protected override Symbol CreateSymbol(string name)
        {
            return new Argument<string>(name);
        }
    }
}