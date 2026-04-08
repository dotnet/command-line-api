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

public class ValueFactoryTests
{
    [Fact]
    public void HasDefaultValue_can_be_set_to_true_when_ValueFactory_handles_missing_tokens()
    {
        Func<ArgumentResult, FileSystemInfo> valueFactory = result => null;
        var argument1 = new Argument<FileSystemInfo>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var argument = argument1;

        argument.HasDefaultValue
                .Should()
                .BeTrue();
    }

    [Fact]
    public void HasDefaultValue_can_be_set_to_false_when_ValueFactory_only_parses_tokens()
    {
        Func<ArgumentResult, FileSystemInfo> valueFactory = result => null;
        var argument1 = new Argument<FileSystemInfo>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        argument.HasDefaultValue
                .Should()
                .BeFalse();
    }

    [Fact]
    public void SetValueFactory_configured_for_parsing_only_does_not_make_a_required_argument_optional()
    {
        Func<ArgumentResult, int> valueFactory = result => int.Parse(result.Tokens.Single().Value);
        var argument1 = new Argument<int>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("")
                                   .Errors
                                   .Should()
                                   .ContainSingle(e => ((ArgumentResult)e.SymbolResult).Argument == argument);
    }

    [Fact]
    public void GetDefaultValue_returns_specified_value_when_ValueFactory_handles_missing_tokens()
    {
        Func<ArgumentResult, string> valueFactory = result => "the-default";
        var argument1 = new Argument<string>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var argument = argument1;

        argument.GetDefaultValue()
                .Should()
                .Be("the-default");
    }

    [Fact]
    public void GetDefaultValue_can_return_null_when_ValueFactory_handles_missing_tokens()
    {
        Func<ArgumentResult, string> valueFactory = result => null;
        var argument1 = new Argument<string>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var argument = argument1;

        argument.GetDefaultValue()
                .Should()
                .BeNull();
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_ValueFactory_parses_tokens()
    {
        Func<ArgumentResult, FileSystemInfo> valueFactory = result =>
        {
            result.AddError("oops!");
            return null;
        };
        var argument1 = new Argument<FileSystemInfo>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("x")
                                   .Errors
                                   .Should()
                                   .ContainSingle(e => ((ArgumentResult)e.SymbolResult).Argument == argument)
                                   .Which
                                   .Message
                                   .Should()
                                   .Be("oops!");
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_ValueFactory_evaluates_default_argument_value()
    {
        Func<ArgumentResult, FileSystemInfo> valueFactory = result =>
        {
            result.AddError("oops!");
            return null;
        };
        var argument1 = new Argument<FileSystemInfo>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("")
                                   .Errors
                                   .Should()
                                   .ContainSingle(e => ((ArgumentResult)e.SymbolResult).Argument == argument)
                                   .Which
                                   .Message
                                   .Should()
                                   .Be("oops!");
    }

    [Fact]
    public void Validation_failure_message_can_be_specified_when_ValueFactory_evaluates_default_option_value()
    {
        Func<ArgumentResult, FileSystemInfo> valueFactory = result =>
        {
            result.AddError("oops!");
            return null;
        };
        var option1 = new Option<FileSystemInfo>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var option = option1;

        new RootCommand { option }.Parse("")
                                 .Errors
                                 .Should()
                                 .ContainSingle()
                                 .Which
                                 .Message
                                 .Should()
                                 .Be("oops!");
    }

    [Fact]
    public void ValueFactory_can_parse_scalar_value_from_an_argument_with_one_token()
    {
        Func<ArgumentResult, int> valueFactory = result => int.Parse(result.Tokens.Single().Value);
        var argument1 = new Argument<int>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("123")
                                   .GetValue(argument)
                                   .Should()
                                   .Be(123);
    }

    [Fact]
    public void ValueFactory_can_parse_sequence_value_from_an_argument_with_one_token()
    {
        Func<ArgumentResult, IEnumerable<int>> valueFactory = result => result.Tokens.Single().Value.Split(',').Select(int.Parse);
        var argument1 = new Argument<IEnumerable<int>>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("1,2,3")
                                   .GetValue(argument)
                                   .Should()
                                   .BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ValueFactory_can_parse_sequence_value_from_an_argument_with_multiple_tokens()
    {
        Func<ArgumentResult, IEnumerable<int>> valueFactory = result => result.Tokens.Select(t => int.Parse(t.Value)).ToArray();
        var argument1 = new Argument<IEnumerable<int>>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        new RootCommand { argument }.Parse("1 2 3")
                                   .GetValue(argument)
                                   .Should()
                                   .BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void ValueFactory_can_parse_scalar_value_from_an_argument_with_multiple_tokens()
    {
        Func<ArgumentResult, int> valueFactory = result => result.Tokens.Select(t => int.Parse(t.Value)).Sum();
        var argument1 = new Argument<int>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;
        argument.Arity = ArgumentArity.ZeroOrMore;

        new RootCommand { argument }.Parse("1 2 3")
                                   .GetValue(argument)
                                   .Should()
                                   .Be(6);
    }

    [Fact]
    public void Option_ArgumentResult_Parent_is_set_correctly_when_token_is_implicit_for_ValueFactory()
    {
        ArgumentResult argumentResult = null;

        Func<ArgumentResult, string> valueFactory = argResult =>
        {
            argumentResult = argResult;
            return null;
        };
        var option1 = new Option<string>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var option = option1;

        var command = new Command("the-command")
        {
            option
        };

        command.Parse("");

        argumentResult
            .Parent
            .Should()
            .BeOfType<OptionResult>()
            .Which
            .Option
            .Should()
            .BeSameAs(command.Options.Single());
    }

    [Fact]
    public void Option_ArgumentResult_parentage_to_root_symbol_is_set_correctly_when_token_is_implicit_for_ValueFactory()
    {
        ArgumentResult argumentResult = null;

        Func<ArgumentResult, string> valueFactory = argResult =>
        {
            argumentResult = argResult;
            return null;
        };
        var option1 = new Option<string>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var option = option1;

        var command = new Command("the-command")
        {
            option
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
    public void Symbol_can_be_found_without_explicitly_traversing_result_tree_from_ValueFactory(string commandLine)
    {
        SymbolResult resultForOptionX = null;
        Func<ArgumentResult, string> valueFactory = _ => string.Empty;
        var option = new Option<string>("-x", new string[0]);
        option.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var optionX = option;

        Func<ArgumentResult, string> valueFactory1 = argResult =>
        {
            resultForOptionX = argResult.GetResult(optionX);
            return string.Empty;
        };
        var option1 = new Option<string>("-y", new string[0]);
        option1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var optionY = option1;

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
    public void Command_ArgumentResult_Parent_is_set_correctly_when_token_is_implicit_for_ValueFactory()
    {
        ArgumentResult argumentResult = null;

        Func<ArgumentResult, string> valueFactory = argResult =>
        {
            argumentResult = argResult;
            return null;
        };
        var argument1 = new Argument<string>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var argument = argument1;

        var command = new Command("the-command")
        {
            argument
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
    public async Task ValueFactory_for_parsing_is_only_called_once()
    {
        var callCount = 0;
        var handlerWasCalled = false;

        Func<ArgumentResult, int> valueFactory = result =>
        {
            callCount++;
            return int.Parse(result.Tokens.Single().Value);
        };
        var option1 = new Option<int>("--value", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var option = option1;

        var command = new RootCommand();
        command.SetAction((ctx) => handlerWasCalled = true);
        command.Options.Add(option);

        await command.Parse("--value 42").InvokeAsync();

        callCount.Should().Be(1);
        handlerWasCalled.Should().BeTrue();
    }

    [Fact]
    public void ValueFactory_can_be_used_together_for_default_value_and_custom_argument_parsing()
    {
        Func<ArgumentResult, int> valueFactory = result => result.Tokens.Count == 0 ? 123 : 789;

        var argument1 = new Argument<int>("arg");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.Always);
        var argument = argument1;

        var result = new RootCommand { argument }.Parse("");

        result.GetValue(argument)
              .Should()
              .Be(123);
    }

    [Fact]
    public void Multiple_arguments_can_have_ValueFactories()
    {
        Func<ArgumentResult, FileInfo[]> valueFactory = argumentResult =>
        {
            argumentResult.AddError("nope");
            return null;
        };
        var argument = new Argument<FileInfo[]>("from");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        Func<ArgumentResult, DirectoryInfo> valueFactory1 = argumentResult =>
        {
            argumentResult.AddError("UH UH");
            return null;
        };
        var argument1 = new Argument<DirectoryInfo>("to");
        argument1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var root = new RootCommand
        {
            argument,
            argument1
        };

        root.Arguments[0].Arity = new ArgumentArity(0, 2);
        root.Arguments[1].Arity = ArgumentArity.ExactlyOne;

        var result = root.Parse("a.txt b.txt /path/to/dir");

        result.Errors.Select(e => e.Message).Should().Contain("nope");
        result.Errors.Select(e => e.Message).Should().Contain("UH UH");
    }

    [Theory]
    [InlineData("--option-with-error 123 --depends-on-option-with-error")]
    [InlineData("--depends-on-option-with-error --option-with-error 123")]
    public void ValueFactory_can_check_another_option_result_for_custom_errors(string commandLine)
    {
        Func<ArgumentResult, string> valueFactory = r =>
        {
            r.AddError("one");
            return r.Tokens[0].Value;
        };
        var option = new Option<string>("--option-with-error", new string[0]);
        option.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var optionWithError = option;

        Func<ArgumentResult, bool> valueFactory1 = result =>
        {
            if (result.GetResult(optionWithError) is { } optionWithErrorResult)
            {
                var otherOptionError = optionWithErrorResult.Errors.SingleOrDefault()?.Message;
                result.AddError(otherOptionError + " " + "two");
            }

            return false;
        };
        var option1 = new Option<bool>("--depends-on-option-with-error", new string[0]);
        option1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var optionThatDependsOnOptionWithError = option1;

        var command = new Command("cmd")
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
    public void Validation_reports_all_parse_errors_when_ValueFactory_is_used()
    {
        Option<string> firstOptionWithError = new("--first-option-with-error");
        firstOptionWithError.Validators.Add(optionResult => optionResult.AddError("first error"));

        Func<ArgumentResult, string> valueFactory = r =>
        {
            r.AddError("second error");
            return r.Tokens[0].Value;
        };
        var option = new Option<string>("--second-option-with-error", new string[0]);
        option.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var secondOptionWithError = option;

        Command command = new("cmd")
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
    public void When_ValueFactory_conversion_fails_then_an_option_does_not_accept_further_arguments()
    {
        Func<ArgumentResult, string> valueFactory = argResult =>
        {
            argResult.AddError("nope");
            return default;
        };
        var option1 = new Option<string>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var option = option1;

        var command = new Command("the-command")
        {
            new Argument<string>("arg"),
            option
        };

        var result = command.Parse("the-command -x nope yep");

        result.CommandResult.Tokens.Count.Should().Be(1);
    }

    [Fact]
    public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws_when_ValueFactory_is_used()
    {
        Func<ArgumentResult, int> valueFactory = argumentResult =>
        {
            if (int.TryParse(argumentResult.Tokens.Select(t => t.Value).Single(), out var value))
            {
                return value;
            }

            argumentResult.AddError($"'{argumentResult.Tokens.Single().Value}' is not an integer");
            return default;
        };
        string[] aliases = new[] { "-o" };
        var option1 = new Option<int>("--one", aliases);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var option = option1;

        var command = new Command("the-command")
        {
            option
        };

        var result = command.Parse("the-command -o not-an-int");

        Action getValue = () => result.GetValue(option);

        getValue.Should()
                .Throw<InvalidOperationException>()
                .Which
                .Message
                .Should()
                .Be("'not-an-int' is not an integer");
    }

    [Fact]
    public void ValueFactory_is_called_once_per_parse_operation_when_input_is_provided()
    {
        var i = 0;

        Func<ArgumentResult, int> valueFactory = result => ++i;
        var option1 = new Option<int>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var option = option1;

        var command = new RootCommand
        {
            option
        };

        command.Parse("-x 123");
        command.Parse("-x 123");

        i.Should().Be(2);
    }

    [Fact]
    public void ValueFactory_is_called_once_per_parse_operation_when_no_input_is_provided()
    {
        var i = 0;

        Func<ArgumentResult, int> valueFactory = result => ++i;
        var option1 = new Option<int>("-x", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensNotMatched);
        var option = option1;

        var command = new RootCommand
        {
            option
        };

        command.Parse("");
        command.Parse("");

        i.Should().Be(2);
    }

    [Theory]
    [InlineData("", "option-is-implicit")]
    [InlineData("--bananas", "argument-is-implicit")]
    [InlineData("--bananas argument-is-specified", "argument-is-specified")]
    public void ValueFactory_when_configured_as_Always_is_called_when_Option_Arity_allows_zero_tokens(string commandLine, string expectedValue)
    {
        Func<ArgumentResult, string> both = result =>
        {
            if (result.Tokens.Count == 0)
            {
                if (result.Parent is OptionResult { Implicit: true })
                {
                    return "option-is-implicit";
                }

                return "argument-is-implicit";
            }

            return result.Tokens[0].Value;
        };

        var option = new Option<string>("--bananas", new string[0]);
        option.SetValueFactory(both, ValueFactoryInvocation.Always);
        var opt = option;
        opt.Arity = ArgumentArity.ZeroOrOne;

        var rootCommand = new RootCommand
        {
            opt
        };

        rootCommand.Parse(commandLine).GetValue(opt).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("1 2 3 4 5 6 7 8")]
    [InlineData("-o 999 1 2 3 4 5 6 7 8")]
    [InlineData("1 2 3 -o 999 4 5 6 7 8")]
    public void ValueFactory_can_pass_on_remaining_tokens(string commandLine)
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(3);

            return new[]
            {
                int.Parse(result.Tokens[0].Value),
                int.Parse(result.Tokens[1].Value),
                int.Parse(result.Tokens[2].Value)
            };
        };
        var argument = new Argument<int[]>("one");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument1 = argument;

        Func<ArgumentResult, int[]> valueFactory1 = result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray();
        var argument3 = new Argument<int[]>("two");
        argument3.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var argument2 = argument3;

        var command = new RootCommand
        {
            argument1,
            argument2,
            new Option<int>("-o")
        };

        var parseResult = command.Parse(commandLine);

        parseResult.GetResult(argument1)
                   .GetValueOrDefault<int[]>()
                   .Should()
                   .BeEquivalentTo(new[] { 1, 2, 3 }, options => options.WithStrictOrdering());

        parseResult.GetResult(argument2)
                   .GetValueOrDefault<int[]>()
                   .Should()
                   .BeEquivalentTo(new[] { 4, 5, 6, 7, 8 }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void ValueFactory_can_return_null()
    {
        Func<ArgumentResult, IPAddress> valueFactory = argumentResult =>
        {
            string value = argumentResult.Tokens.Last().Value;
            if (IPAddress.TryParse(value, out var address))
            {
                return address;
            }

            argumentResult.AddError($"'{value}' is not a valid value");
            return null;
        };
        var option1 = new Option<IPAddress>("-ip", new string[0]);
        option1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var option = option1;

        ParseResult parseResult = new RootCommand() { option }.Parse("-ip a.b.c.d");

        parseResult.Errors.Should().Contain(error => error.Message == "'a.b.c.d' is not a valid value");
    }

    [Fact]
    public void When_tokens_are_passed_on_by_ValueFactory_on_last_argument_then_they_become_unmatched_tokens()
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(3);

            return new[]
            {
                int.Parse(result.Tokens[0].Value),
                int.Parse(result.Tokens[1].Value),
                int.Parse(result.Tokens[2].Value)
            };
        };
        var argument = new Argument<int[]>("one");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument1 = argument;

        var command = new RootCommand
        {
            argument1
        };

        var parseResult = command.Parse("1 2 3 4 5 6 7 8");

        parseResult.UnmatchedTokens
                   .Should()
                   .BeEquivalentTo(new[] { "4", "5", "6", "7", "8" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void When_ValueFactory_passes_on_tokens_the_argument_result_tokens_reflect_the_change()
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(3);

            return new[]
            {
                int.Parse(result.Tokens[0].Value),
                int.Parse(result.Tokens[1].Value),
                int.Parse(result.Tokens[2].Value)
            };
        };
        var argument = new Argument<int[]>("one");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument1 = argument;
        Func<ArgumentResult, int[]> valueFactory1 = result => result.Tokens.Select(t => t.Value).Select(int.Parse).ToArray();
        var argument3 = new Argument<int[]>("two");
        argument3.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var argument2 = argument3;

        var command = new RootCommand
        {
            argument1,
            argument2
        };

        var parseResult = command.Parse("1 2 3 4 5 6 7 8");

        parseResult.GetResult(argument1)
                   .Tokens
                   .Select(t => t.Value)
                   .Should()
                   .BeEquivalentTo(new[] { "1", "2", "3" }, options => options.WithStrictOrdering());

        parseResult.GetResult(argument2)
                   .Tokens
                   .Select(t => t.Value)
                   .Should()
                   .BeEquivalentTo(new[] { "4", "5", "6", "7", "8" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void OnlyTake_throws_when_called_with_a_negative_value_from_ValueFactory()
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(-1);
            return null;
        };
        var argument1 = new Argument<int[]>("one");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        argument.Invoking(a => new RootCommand { a }.Parse("1 2 3"))
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which
                .Message
                .Should()
                .ContainAll("Value must be at least 1.", "Actual value was -1.");
    }

    [Fact]
    public void OnlyTake_throws_when_called_twice_from_ValueFactory()
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(1);
            result.OnlyTake(1);
            return null;
        };
        var argument1 = new Argument<int[]>("one");
        argument1.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument = argument1;

        argument.Invoking(a => new RootCommand { a }.Parse("1 2 3"))
                .Should()
                .Throw<InvalidOperationException>()
                .Which
                .Message
                .Should()
                .Be("OnlyTake can only be called once.");
    }

    [Fact]
    public void OnlyTake_can_pass_on_all_tokens_from_one_multiple_arity_argument_to_another_when_using_ValueFactory()
    {
        Func<ArgumentResult, int[]> valueFactory = result =>
        {
            result.OnlyTake(0);
            return null;
        };
        var argument = new Argument<int[]>("arg1");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var argument1 = argument;
        argument1.Arity = ArgumentArity.ZeroOrMore;

        var argument2 = new Argument<int[]>("arg2");

        var command = new RootCommand
        {
            argument1,
            argument2
        };

        var result = command.Parse("1 2 3");

        result.GetValue(argument1).Should().BeEmpty();
        result.GetValue(argument2).Should().BeEquivalentSequenceTo(1, 2, 3);
    }

    [Fact]
    public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another_when_using_ValueFactory()
    {
        Func<ArgumentResult, int?> valueFactory = ctx =>
        {
            ctx.OnlyTake(0);
            return null;
        };
        var argument = new Argument<int?>("arg");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var scalar = argument;

        Argument<int[]> multiple = new("args");

        var command = new RootCommand
        {
            scalar,
            multiple
        };

        var result = command.Parse("1 2 3");

        result.GetValue(scalar).Should().BeNull();
        result.GetValue(multiple).Should().BeEquivalentSequenceTo(1, 2, 3);
    }

    [Fact]
    public void OnlyTake_can_pass_on_all_tokens_from_a_single_arity_argument_to_another_that_also_passes_them_all_on_when_using_ValueFactory()
    {
        Func<ArgumentResult, string> valueFactory = ctx =>
        {
            ctx.OnlyTake(0);
            return null;
        };
        var argument = new Argument<string>("first");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var first = argument;
        first.Arity = ArgumentArity.ZeroOrOne;

        Func<ArgumentResult, string[]> valueFactory1 = ctx =>
        {
            ctx.OnlyTake(0);
            return null;
        };
        var argument1 = new Argument<string[]>("second");
        argument1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var second = argument1;
        second.Arity = ArgumentArity.ZeroOrMore;

        Func<ArgumentResult, string[]> valueFactory2 = ctx =>
        {
            ctx.OnlyTake(3);
            return new[] { "1", "2", "3" };
        };
        var argument2 = new Argument<string[]>("third");
        argument2.SetValueFactory(valueFactory2, ValueFactoryInvocation.WhenTokensMatched);
        var third = argument2;
        third.Arity = ArgumentArity.ZeroOrMore;

        var command = new RootCommand
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
    public void GetResult_by_name_can_be_used_recursively_within_argument_ValueFactories()
    {
        ArgumentResult firstResult = null;
        ArgumentResult secondResult = null;

        Func<ArgumentResult, string> valueFactory = ctx =>
        {
            secondResult = (ArgumentResult)ctx.GetResult("second");
            return ctx.Tokens.SingleOrDefault()?.Value;
        };
        var argument = new Argument<string>("first");
        argument.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var first = argument;

        Func<ArgumentResult, string> valueFactory1 = ctx =>
        {
            firstResult = (ArgumentResult)ctx.GetResult("first");
            return ctx.Tokens.SingleOrDefault()?.Value;
        };
        var argument1 = new Argument<string>("second");
        argument1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var second = argument1;

        var command = new RootCommand
        {
            first,
            second,
            new Argument<string>("third")
        };

        var result = command.Parse("one two three");

        result.GetValue<string>("third").Should().Be("three");
        firstResult.GetValueOrDefault<string>().Should().Be("one");
        secondResult.GetValueOrDefault<string>().Should().Be("two");
    }

    [Theory]
    [InlineData("--first one --second two --third three")]
    [InlineData("--third three --second two --first one")]
    public void GetResult_by_name_can_be_used_recursively_within_option_ValueFactories(string commandLine)
    {
        OptionResult firstOptionResult = null;
        OptionResult secondOptionResult = null;

        Func<ArgumentResult, string> valueFactory = ctx =>
        {
            secondOptionResult = (OptionResult)ctx.GetResult("--second");
            return ctx.Tokens.SingleOrDefault()?.Value;
        };
        var option = new Option<string>("--first", new string[0]);
        option.SetValueFactory(valueFactory, ValueFactoryInvocation.WhenTokensMatched);
        var first = option;

        Func<ArgumentResult, string> valueFactory1 = ctx =>
        {
            firstOptionResult = (OptionResult)ctx.GetResult("--first");
            return ctx.Tokens.SingleOrDefault()?.Value;
        };
        var option1 = new Option<string>("--second", new string[0]);
        option1.SetValueFactory(valueFactory1, ValueFactoryInvocation.WhenTokensMatched);
        var second = option1;

        var command = new RootCommand
        {
            first,
            second,
            new Option<string>("--third")
        };

        var parseResult = command.Parse(commandLine);

        firstOptionResult.GetValueOrDefault<string>().Should().Be("one");
        secondOptionResult.GetValueOrDefault<string>().Should().Be("two");
        parseResult.GetValue<string>("--first").Should().Be("one");
        parseResult.GetValue<string>("--second").Should().Be("two");
        parseResult.GetValue<string>("--third").Should().Be("three");
    }
}
