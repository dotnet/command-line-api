// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using System.Linq;
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
            public void Validation_failure_message_can_be_specified()
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
            public void ArgumentResult_Parent_is_set_correctly_when_token_is_present()
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
                        })
                };

                command.Parse("-x abc");

                argumentResult
                    .Parent
                    .Should()
                    .NotBeNull();
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
        }

        protected override Symbol CreateSymbol(string name)
        {
            return new Argument(name);
        }
    }
}