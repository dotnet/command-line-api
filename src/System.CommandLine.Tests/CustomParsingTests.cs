// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class CustomParsingTests
{
    [Fact]
    public void HasDefaultValue_can_be_set_to_true()
    {
        var argument = new CliArgument<FileSystemInfo>("arg")
        {
            DefaultValueFactory = result => null
        };

        argument.HasDefaultValue
                .Should()
                .BeTrue();
    }

    [Fact]
    public void HasDefaultValue_can_be_set_to_false()
    {
        var argument = new CliArgument<FileSystemInfo>("arg")
        {
            DefaultValueFactory = null
        };

        argument.HasDefaultValue
                .Should()
                .BeFalse();
    }

    [Fact]
    public void GetDefaultValue_returns_specified_value()
    {
        var argument = new CliArgument<string>("arg")
        {
            DefaultValueFactory = result => "the-default"
        };

        argument.GetDefaultValue()
                .Should()
                .Be("the-default");
    }

    [Fact]
    public void GetDefaultValue_returns_null_when_custom_parser_returns_true_without_setting_a_value()
    {
        var argument = new CliArgument<string>("arg")
        {
            DefaultValueFactory = result => null
        };

        argument.GetDefaultValue()
                .Should()
                .BeNull();
    }

    [Fact]
    public void GetDefaultValue_can_return_null()
    {
        var argument = new CliArgument<string>("arg")
        {
            DefaultValueFactory = result => null
        };

        argument.GetDefaultValue()
                .Should()
                .BeNull();
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_parsing_tokens()
    {
        var argument = new CliArgument<FileSystemInfo>("arg")
        {
            CustomParser = result =>
            {
                result.AddError("oops!");
                return null;
            }
        };

        new CliRootCommand { argument }.Parse("x")
                                       .Errors
                                       .Should()
                                       .ContainSingle(e => ((ArgumentResult)e.SymbolResult).Argument == argument)
                                       .Which
                                       .Message
                                       .Should()
                                       .Be("oops!");
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_evaluating_default_argument_value()
    {
        var argument = new CliArgument<FileSystemInfo>("arg")
        {
            DefaultValueFactory = result =>
            {
                result.AddError("oops!");
                return null;
            }
        };

        new CliRootCommand { argument }.Parse("")
                                       .Errors
                                       .Should()
                                       .ContainSingle(e => ((ArgumentResult)e.SymbolResult).Argument == argument)
                                       .Which
                                       .Message
                                       .Should()
                                       .Be("oops!");
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_evaluating_default_option_value()
    {
        var option = new CliOption<FileSystemInfo>("-x")
        {
            DefaultValueFactory = result =>
            {
                result.AddError("oops!");
                return null;
            }
        };

        new CliRootCommand { option }.Parse("")
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
        var argument = new CliArgument<int>("arg")
        {
            CustomParser = result => int.Parse(result.Tokens.Single().Value)
        };

        new CliRootCommand { argument }.Parse("123")
                                       .GetValue(argument)
                                       .Should()
                                       .Be(123);
    }

    [Fact]
    public void custom_parsing_of_sequence_value_from_an_argument_with_one_token()
    {
        var argument = new CliArgument<IEnumerable<int>>("arg")
        {
            CustomParser = result => result.Tokens.Single().Value.Split(',').Select(int.Parse)
        };

        new CliRootCommand { argument }.Parse("1,2,3")
                                       .GetValue(argument)
                                       .Should()
                                       .BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void custom_parsing_of_sequence_value_from_an_argument_with_multiple_tokens()
    {
        var argument = new CliArgument<IEnumerable<int>>("arg")
        {
            CustomParser = result => result.Tokens.Select(t => int.Parse(t.Value)).ToArray()
        };

        new CliRootCommand { argument }.Parse("1 2 3")
                                       .GetValue(argument)
                                       .Should()
                                       .BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void custom_parsing_of_scalar_value_from_an_argument_with_multiple_tokens()
    {
        var argument = new CliArgument<int>("arg")
        {
            CustomParser = result => result.Tokens.Select(t => int.Parse(t.Value)).Sum(),
            Arity = ArgumentArity.ZeroOrMore
        };

        new CliRootCommand { argument }.Parse("1 2 3")
                                       .GetValue(argument)
                                       .Should()
                                       .Be(6);
    }

    [Fact]
    public void Option_ArgumentResult_Parent_is_set_correctly_when_token_is_implicit()
    {
        ArgumentResult argumentResult = null;

        var command = new CliCommand("the-command")
        {
            new CliOption<string>("-x")
            {
                DefaultValueFactory = argResult =>
                {
                    argumentResult = argResult;
                    return null;
                }
            }
        };

        CliConfiguration simpleConfig = new (command);
        command.Parse("", simpleConfig);

        argumentResult
            .Parent
            .Should()
            .BeOfType<OptionResult>()
            .Which
            .Option
            .Should()
            .Be(command.Options.Single());
    }

    [Fact]
    public void Option_ArgumentResult_parentage_to_root_symbol_is_set_correctly_when_token_is_implicit()
    {
        ArgumentResult argumentResult = null;

        var command = new CliCommand("the-command")
        {
            new CliOption<string>("-x")
            {
                DefaultValueFactory = argResult =>
                {
                    argumentResult = argResult;
                    return null;
                }
            }
        };

        command.Parse("");

        argumentResult
            .Parent
            .Parent
            .Should()
            .BeOfType<CommandResult>()
            .Which
            .Command
            .Should()
            .BeSameAs(command);
    }
            
    [Theory]
    [InlineData("-x value-x -y value-y")]
    [InlineData("-y value-y -x value-x")]
    public void Symbol_can_be_found_without_explicitly_traversing_result_tree(string commandLine)
    {
        SymbolResult resultForOptionX = null;
        var optionX = new CliOption<string>("-x")
        {
            CustomParser = _ => string.Empty
        };
                
        var optionY = new CliOption<string>("-y")
        {
            CustomParser = argResult =>
            {
                resultForOptionX = argResult.GetResult(optionX);
                return string.Empty;
            }
        };
            
        var command = new CliCommand("the-command")
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

        var command = new CliCommand("the-command")
        {
            new CliArgument<string>("arg")
            {
                DefaultValueFactory = argResult =>
                {
                    argumentResult = argResult;
                    return null;
                }
            }
        };

        command.Parse("");

        argumentResult
            .Parent
            .Should()
            .BeOfType<CommandResult>()
            .Which
            .Command
            .Should()
            .BeSameAs(command);
    }

    [Fact]
    public async Task Custom_argument_parser_is_only_called_once()
    {
        var callCount = 0;
        var handlerWasCalled = false;

        var option = new CliOption<int>("--value")
        {
            CustomParser = result =>
            {
                callCount++;
                return int.Parse(result.Tokens.Single().Value);
            }
        };

        var command = new CliRootCommand();
        command.SetAction((ctx) => handlerWasCalled = true);
        command.Options.Add(option);

        await command.Parse("--value 42").InvokeAsync();

        callCount.Should().Be(1);
        handlerWasCalled.Should().BeTrue();
    }

    [Fact]
    public void Default_value_and_custom_argument_parser_can_be_used_together()
    {
        var argument = new CliArgument<int>("arg")
        {
            CustomParser = _ => 789,
            DefaultValueFactory = _ => 123
        };

        var result = new CliRootCommand { argument }.Parse("");

        result.GetValue(argument)
              .Should()
              .Be(123);
    }

    [Fact]
    public void Multiple_arguments_can_have_custom_parsers()
    {
        var root = new CliRootCommand
        {
            new CliArgument<FileInfo[]>("from")
            {
                CustomParser = argumentResult =>
                {
                    argumentResult.AddError("nope");
                    return null;
                },
                Arity = new ArgumentArity(0, 2)
            },
            new CliArgument<DirectoryInfo>("to")
            {
                CustomParser = argumentResult =>
                {
                    argumentResult.AddError("UH UH");
                    return null;
                },
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

    [Theory]
    // The two different examples verify that the relative order of the symbols doesn't matter.
    [InlineData("--option-with-error 123 --depends-on-option-with-error")]
    [InlineData("--depends-on-option-with-error --option-with-error 123")]
    public void Custom_parser_can_check_another_option_result_for_custom_errors(string commandLine)
    {
        var optionWithError = new CliOption<string>("--option-with-error")
        {
            CustomParser = r =>
            {
                r.AddError("one");
                return r.Tokens[0].Value;
            }
        };

        var optionThatDependsOnOptionWithError = new CliOption<bool>("--depends-on-option-with-error")
        {
            CustomParser = result =>
            {
                if (result.GetResult(optionWithError) is { } optionWithErrorResult)
                {
                    var otherOptionError = optionWithErrorResult.Errors.SingleOrDefault()?.Message;

                    result.AddError(otherOptionError + " " + "two");
                }

                return false;
            }
        };

        var command = new CliCommand("cmd")
        {
            optionWithError,
            optionThatDependsOnOptionWithError
        };

        var parseResult = command.Parse(commandLine);

        parseResult.Errors
                   .Single(e => e.SymbolResult is OptionResult optResult &&
                                optResult.Option == optionThatDependsOnOptionWithError)
                   .Message
                   .Should()
                   .Be("one two");
    }

    [Fact]
    public void Validation_reports_all_parse_errors()
    {
        CliOption<string> firstOptionWithError = new("--first-option-with-error");
        firstOptionWithError.Validators.Add(optionResult => optionResult.AddError("first error"));
        CliOption<string> secondOptionWithError = new("--second-option-with-error")
        {
            CustomParser = r =>
            {
                r.AddError("second error");
                return r.Tokens[0].Value;
            }
        };

        CliCommand command = new ("cmd")
        {
            firstOptionWithError,
            secondOptionWithError
        };

        ParseResult parseResult = command.Parse("cmd --first-option-with-error value1 --second-option-with-error value2");

        OptionResult firstOptionResult = parseResult.GetResult(firstOptionWithError);
        firstOptionResult.Errors.Single().Message.Should().Be("first error");

        OptionResult secondOptionResult = parseResult.GetResult(secondOptionWithError);
        secondOptionResult.Errors.Single().Message.Should().Be("second error");

        parseResult.Errors.Should().Contain(error => error.SymbolResult == firstOptionResult);
        parseResult.Errors.Should().Contain(error => error.SymbolResult == secondOptionResult);
    }

    [Fact]
    public void When_custom_conversion_fails_then_an_option_does_not_accept_further_arguments()
    {
        var command = new CliCommand("the-command")
        {
            new CliArgument<string>("arg"),
            new CliOption<string>("-x")
            {
                CustomParser = argResult =>
                {
                    argResult.AddError("nope");
                    return default;
                }
            }
        };

        var result = command.Parse("the-command -x nope yep");

        result.CommandResult.Tokens.Count.Should().Be(1);
    }
            
    [Fact]
    public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
    {
        var option = new CliOption<int>("--one", "-o")
        {
            CustomParser = argumentResult =>
            {
                if (int.TryParse(argumentResult.Tokens.Select(t => t.Value).Single(), out var value))
                {
                    return value;
                }

                argumentResult.AddError($"'{argumentResult.Tokens.Single().Value}' is not an integer");

                return default;
            }
        };

        var command = new CliCommand("the-command")
        {
            option
        };

        var result = command.Parse("the-command -o not-an-int");

        Action getValue = () => 
            result.GetValue(option);

        getValue.Should()
                .Throw<InvalidOperationException>()
                .Which
                .Message
                .Should()
                .Be("'not-an-int' is not an integer");
    }

    [Fact]
    public void Custom_parser_is_called_once_per_parse_operation_when_input_is_provided()
    {
        var i = 0;

        var command = new CliRootCommand
        {
            new CliOption<int>("-x")
            {
                CustomParser = result => ++i,
            }
        };

        command.Parse("-x 123");
        command.Parse("-x 123");

        i.Should().Be(2);
    }

    [Fact]
    public void Default_value_factory_is_called_once_per_parse_operation_when_no_input_is_provided()
    {
        var i = 0;

        var command = new CliRootCommand
        {
            new CliOption<int>("-x")
            {
                DefaultValueFactory = result => ++i,
            }
        };

        command.Parse("");
        command.Parse("");

        i.Should().Be(2);
    }

    [Theory] // https://github.com/dotnet/command-line-api/issues/1294
    [InlineData("", "option-is-implicit")]
    [InlineData("--bananas", "argument-is-implicit")]
    [InlineData("--bananas argument-is-specified", "argument-is-specified")]
    public void Custom_parser_when_configured_as_default_value_factory_is_called_when_Option_Arity_allows_zero_tokens(string commandLine, string expectedValue)
    {
        Func<ArgumentResult, string> both = (result) =>
        {
            if (result.Tokens.Count == 0)
            {
                if (result.Parent is OptionResult { Implicit: true })
                {
                    return "option-is-implicit";
                }

                return "argument-is-implicit";
            }
            else
            {
                return result.Tokens[0].Value;
            }
        };

        var opt = new CliOption<string>("--bananas")
        {
            DefaultValueFactory = both,
            CustomParser = both,
            Arity = ArgumentArity.ZeroOrOne
        };

        var rootCommand = new CliRootCommand
        {
            opt
        };

        rootCommand.Parse(commandLine).GetValue(opt).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("1 2 3 4 5 6 7 8")]
    [InlineData("-o 999 1 2 3 4 5 6 7 8")]
    [InlineData("1 2 3 -o 999 4 5 6 7 8")]
    public void Custom_parser_can_pass_on_remaining_tokens(string commandLine)
    {
        var argument1 = new CliArgument<int[]>("one")
        {
            CustomParser = result =>
            {
                result.OnlyTake(3);

                return new[]
                {
                    int.Parse(result.Tokens[0].Value),
                    int.Parse(result.Tokens[1].Value),
                    int.Parse(result.Tokens[2].Value)
                };
            }
        };
        var argument2 = new CliArgument<int[]>("two")
        {
            CustomParser = result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray()
        };
        var command = new CliRootCommand
        {
            argument1,
            argument2,
            new CliOption<int>("-o")
        };

        var parseResult = command.Parse(commandLine);

        parseResult.GetResult(argument1)
                   .GetValueOrDefault<int[]>()
                   .Should()
                   .BeEquivalentTo(new[] { 1, 2, 3 },
                                   options => options.WithStrictOrdering());

        parseResult.GetResult(argument2)
                   .GetValueOrDefault<int[]>()
                   .Should()
                   .BeEquivalentTo(new[] { 4, 5, 6, 7, 8 },
                                   options => options.WithStrictOrdering());
    }

    [Fact]
    public void Custom_parser_can_return_null()
    {
        CliOption<IPAddress> option = new("-ip")
        {
            CustomParser = (argumentResult) =>
            {
                string value = argumentResult.Tokens.Last().Value;
                if (IPAddress.TryParse(value, out var address))
                {
                    return address;
                }

                argumentResult.AddError($"'{value}' is not a valid value");
                return null;
            }
        };

        ParseResult parseResult = new CliRootCommand() { option }.Parse("-ip a.b.c.d");

        parseResult.Errors.Should().Contain(error => error.Message == "'a.b.c.d' is not a valid value");
    }

    [Fact]
    public void When_tokens_are_passed_on_by_custom_parser_on_last_argument_then_they_become_unmatched_tokens()
    {

        var argument1 = new CliArgument<int[]>("one")
        {
            CustomParser = result =>
            {
                result.OnlyTake(3);

                return new[]
                {
                    int.Parse(result.Tokens[0].Value),
                    int.Parse(result.Tokens[1].Value),
                    int.Parse(result.Tokens[2].Value)
                };
            }
        };
             
        var command = new CliRootCommand
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
        var argument1 = new CliArgument<int[]>("one")
        {
            CustomParser = result =>
            {
                result.OnlyTake(3);

                return new[]
                {
                    int.Parse(result.Tokens[0].Value),
                    int.Parse(result.Tokens[1].Value),
                    int.Parse(result.Tokens[2].Value)
                };
            }
        };
        var argument2 = new CliArgument<int[]>("two")
        {
            CustomParser = result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray()
        };
        var command = new CliRootCommand
        {
            argument1,
            argument2
        };

        var parseResult = command.Parse("1 2 3 4 5 6 7 8");

        parseResult.GetResult(argument1)
                   .Tokens
                   .Select(t => t.Value)
                   .Should()
                   .BeEquivalentTo(new[] { "1", "2", "3" },
                                   options => options.WithStrictOrdering());

        parseResult.GetResult(argument2)
                   .Tokens
                   .Select(t => t.Value)
                   .Should()
                   .BeEquivalentTo(new[] { "4", "5", "6", "7", "8" },
                                   options => options.WithStrictOrdering());
    }

    [Fact]
    public void OnlyTake_throws_when_called_with_a_negative_value()
    {
        var argument = new CliArgument<int[]>("one")
        {
            CustomParser = result =>
            {
                result.OnlyTake(-1);

                return null;
            }
        };

        argument.Invoking(a => new CliRootCommand { a }.Parse("1 2 3"))
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
        var argument = new CliArgument<int[]>("one")
        {
            CustomParser = result =>
            {
                result.OnlyTake(1);
                result.OnlyTake(1);

                return null;
            }
        };

        argument.Invoking(a => new CliRootCommand { a }.Parse("1 2 3"))
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
        var argument1 = new CliArgument<int[]>("arg1")
        {
            CustomParser = result =>
            {
                result.OnlyTake(0);
                return null;
            }
        };
        var argument2 = new CliArgument<int[]>("arg2");
        var command = new CliRootCommand
        {
            argument1,
            argument2
        };

        var result = command.Parse("1 2 3");

        result.GetValue(argument1).Should().BeEmpty();

        result.GetValue(argument2).Should().BeEquivalentSequenceTo(1, 2, 3);
    }

    [Fact] // https://github.com/dotnet/command-line-api/issues/1759 
    public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another()
    {
        var scalar = new CliArgument<int?>("arg")
        {
            CustomParser = ctx =>
            {
                ctx.OnlyTake(0);
                return null;
            }
        };
        CliArgument<int[]> multiple = new("args");

        var command = new CliRootCommand
        {
            scalar,
            multiple
        };

        var result = command.Parse("1 2 3");

        result.GetValue(scalar).Should().BeNull();

        result.GetValue(multiple).Should().BeEquivalentSequenceTo(1, 2, 3);
    }


    [Fact] //https://github.com/dotnet/command-line-api/issues/1779
    public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another_that_also_passes_them_all_on()
    {
        var first = new CliArgument<string>("first")
        {
            CustomParser = ctx =>
            {
                ctx.OnlyTake(0);
                return null;
            },
            Arity = ArgumentArity.ZeroOrOne
        };

        var second = new CliArgument<string[]>(name: "second")
        {
            CustomParser = ctx =>
            {
                ctx.OnlyTake(0);
                return null;
            },
            Arity = ArgumentArity.ZeroOrMore
        };

        var third = new CliArgument<string[]>(name: "third")
        {
            CustomParser = ctx =>
            {
                ctx.OnlyTake(3);
                return new[] { "1", "2", "3" };
            },
            Arity = ArgumentArity.ZeroOrMore
        };

        var command = new CliRootCommand
        {
            first,
            second,
            third
        };

        var result = command.Parse("1 2 3");

        result.GetValue(first).Should().BeNull();
        result.GetValue(second).Should().BeEmpty();
        result.GetValue(third).Should().BeEquivalentSequenceTo("1", "2", "3");
    }

    [Fact]
    public void GetResult_by_name_can_be_used_recursively_within_argument_custom_parsers()
    {
        ArgumentResult firstResult = null;
        ArgumentResult secondResult = null;

        var command = new CliRootCommand
        {
            new CliArgument<string>("first")
            {
                CustomParser = ctx =>
                {
                    secondResult = (ArgumentResult)ctx.GetResult("second");

                    return ctx.Tokens.SingleOrDefault()?.Value;
                },
            },

            new CliArgument<string>("second")
            {
                CustomParser = ctx =>
                {
                    firstResult = (ArgumentResult)ctx.GetResult("first");

                    return ctx.Tokens.SingleOrDefault()?.Value;
                },
            },

            new CliArgument<string>("third")
        };

        var result = command.Parse("one two three");

        result.GetValue<string>("third").Should().Be("three");
        firstResult.GetValueOrDefault<string>().Should().Be("one");
        secondResult.GetValueOrDefault<string>().Should().Be("two");
    }
    
    [Theory]
    [InlineData("--first one --second two --third three")]
    [InlineData("--third three --second two --first one")]
    public void GetResult_by_name_can_be_used_recursively_within_custom_option_parsers(string commandLine)
    {
        OptionResult firstOptionResult = null;
        OptionResult secondOptionResult = null;

        var command = new CliRootCommand
        {
            new CliOption<string>("--first")
            {
                CustomParser = ctx =>
                {
                    secondOptionResult = (OptionResult)ctx.GetResult("--second");

                    return ctx.Tokens.SingleOrDefault()?.Value;
                },
            },

            new CliOption<string>("--second")
            {
                CustomParser = ctx =>
                {
                    firstOptionResult = (OptionResult)ctx.GetResult("--first");

                    return ctx.Tokens.SingleOrDefault()?.Value;
                },
            },

            new CliOption<string>("--third")
        };

        var parseResult = command.Parse(commandLine);

        firstOptionResult.GetValueOrDefault<string>().Should().Be("one");
        secondOptionResult.GetValueOrDefault<string>().Should().Be("two");
        parseResult.GetValue<string>("--first").Should().Be("one");
        parseResult.GetValue<string>("--second").Should().Be("two");
        parseResult.GetValue<string>("--third").Should().Be("three");
    }
}