﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolResultTests
    {
        [Fact]
        public void An_option_with_a_default_value_and_no_explicitly_provided_argument_has_an_empty_arguments_property()
        {
            var option = new Option("-x")
            {
                Argument = new Argument<string>("default")
            };

            var result = new RootCommand
            {
                option
            }.Parse("-x")
             .FindResultFor(option);

            result.Tokens.Should().BeEmpty();
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_option()
        {
            var command = new Command("the-command")
            {
                new Option(new[] { "-h", "--help" })
            };

            var result = command.Parse("the-command -h");

            result.HasOption("help").Should().BeTrue();
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_implicit_option()
        {
            var command = new Command("the-command")
            {
                new Option(new[] { "-c", "--count" })
                {
                    Argument = new Argument<int>(() => 5)
                }
            };

            var result = command.Parse("the-command");

            result.HasOption("count").Should().BeTrue();
        }

        [Fact]
        public void Command_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var command = new Command("outer")
            {
                new Command("inner-one")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.Zero
                    }
                },
                new Command("inner-two")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.Zero
                    }
                }
            };

            var result = new Parser(command).Parse("outer inner-one inner-two");

            result.CommandResult.Symbol.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = new Parser(command).Parse("outer inner-two inner-one");

            result2.CommandResult.Symbol.Name.Should().Be("inner-two");
            result2.Errors.Count.Should().Be(1);
        }

        [Fact]
        public void ValueForOption_throws_with_empty_alias()
        {
            var command = new Command("one");

            var result = command.Parse("");

            Action action = () =>
            {
                result.ValueForOption<string>(string.Empty);
            };

            action.Should().Throw<ArgumentException>("Value cannot be null or whitespace.");
        }
    }
}
