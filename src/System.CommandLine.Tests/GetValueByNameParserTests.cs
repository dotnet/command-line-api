// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class GetValueByNameParserTests : ParserTests
    {
        public GetValueByNameParserTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override T GetValue<T>(ParseResult parseResult, Option<T> option)
            => parseResult.GetValue<T>(option.Name);

        protected override T GetValue<T>(ParseResult parseResult, Argument<T> argument)
            => parseResult.GetValue<T>(argument.Name);

        [Fact]
        public void In_case_of_argument_name_conflict_the_value_which_belongs_to_the_last_parsed_command_is_returned()
        {
            RootCommand command = new()
            {
                new Argument<int>("arg"),
                new Command("inner1")
                {
                    new Argument<int>("arg"),
                    new Command("inner2")
                    {
                        new Argument<int>("arg"),
                    }
                }
            };

            ParseResult parseResult = command.Parse("1 inner1 2 inner2 3");

            parseResult.GetValue<int>("arg").Should().Be(3);
        }

        [Fact]
        public void In_case_of_option_name_conflict_the_value_which_belongs_to_the_last_parsed_command_is_returned()
        {
            RootCommand command = new()
            {
                new Option<int>("--integer", "-i"),
                new Command("inner1")
                {
                    new Option<int>("--integer", "-i"),
                    new Command("inner2")
                    {
                        new Option<int>("--integer", "-i")
                    }
                }
            };

            ParseResult parseResult = command.Parse("-i 1 inner1 --integer 2 inner2 -i 3");

            parseResult.GetValue<int>("--integer").Should().Be(3);
        }

        [Fact]
        public void When_option_value_is_not_parsed_then_default_value_is_returned()
        {
            RootCommand command = new()
            {
                new Option<int>("--integer", "-i")
            };

            ParseResult parseResult = command.Parse("");

            parseResult.GetValue<int>("--integer").Should().Be(default);
        }

        [Fact]
        public void When_optional_argument_is_not_parsed_then_default_value_is_returned()
        {
            RootCommand command = new()
            {
                new Argument<int>("arg")
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            ParseResult parseResult = command.Parse("");

            parseResult.GetValue<int>("arg").Should().Be(default);
        }

        [Fact]
        public void When_required_option_value_is_not_parsed_then_an_exception_is_thrown()
        {
            RootCommand command = new()
            {
                new Option<int>("--required")
                {
                    IsRequired = true
                }
            };

            ParseResult parseResult = command.Parse("");

            Action getRequired = () => parseResult.GetValue<int>("--required");

            getRequired
                .Should()
                .Throw<InvalidOperationException>()
                .Where(ex => ex.Message == LocalizationResources.RequiredOptionWasNotProvided("--required"));
        }

        [Fact]
        public void When_required_argument_value_is_not_parsed_then_an_exception_is_thrown()
        {
            RootCommand command = new()
            {
                new Argument<int>("required")
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            ParseResult parseResult = command.Parse("");

            Action getRequired = () => parseResult.GetValue<int>("required");

            getRequired
                .Should()
                .Throw<InvalidOperationException>()
                .Where(ex => ex.Message == LocalizationResources.RequiredArgumentMissing(parseResult.FindResultFor(command.Arguments[0])));
        }

        [Fact]
        public void When_non_existing_name_is_used_then_exception_is_thrown()
        {
            ParseResult parseResult = new Command("noSymbols").Parse("");

            Action getRequired = () => parseResult.GetValue<int>("required");

            getRequired
                .Should()
                .Throw<InvalidOperationException>()
                .Where(ex => ex.Message == LocalizationResources.RequiredArgumentMissing(parseResult.FindResultFor(command.Arguments[0])));
        }
    }
}
