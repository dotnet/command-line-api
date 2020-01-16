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
                var argument = new Argument<FileSystemInfo>(result => true, true);

                argument.HasDefaultValue
                        .Should()
                        .BeTrue();
            }

            [Fact]
            public void HasDefaultValue_can_be_set_to_false()
            {
                var argument = new Argument<FileSystemInfo>(result => true, false);

                argument.HasDefaultValue
                        .Should()
                        .BeFalse();
            }

            [Fact]
            public void GetDefaultValue_returns_specified_value()
            {
                var argument = new Argument<string>(result =>
                {
                    result.Value = "the-default";
                    return true;
                }, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .Be("the-default");
            }

            [Fact]
            public void GetDefaultValue_returns_null_when_parse_delegate_returns_true_without_setting_a_value()
            {
                var argument = new Argument<string>(result => { return true; }, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void GetDefaultValue_returns_null_when_parse_delegate_returns_true_and_sets_value_to_null()
            {
                var argument = new Argument<string>(result => { return true; }, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void GetDefaultValue_can_return_null()
            {
                var argument = new Argument<string>(result => { return true; }, isDefault: true);

                argument.GetDefaultValue()
                        .Should()
                        .BeNull();
            }

            [Fact]
            public void validation_failure_message()
            {
                var argument = new Argument<FileSystemInfo>(result =>
                {
                    result.ErrorMessage = "oops!";
                    return true;
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
                var argument = new Argument<int>(result =>
                {
                    result.Value = int.Parse(result.Tokens.Single().Value);

                    return true;
                });

                argument.Parse("123")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .Be(123);
            }

            [Fact]
            public void custom_parsing_of_sequence_value_from_an_argument_with_one_token()
            {
                var argument = new Argument<IEnumerable<int>>(result =>
                {
                    result.Value = result.Tokens.Single().Value.Split(',').Select(int.Parse);

                    return true;
                });

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
                    result.Value = result.Tokens.Select(t => int.Parse(t.Value)).ToArray();
                    return true;
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
                var argument = new Argument<int>(result =>
                {
                    result.Value = result.Tokens.Select(t => int.Parse(t.Value)).Sum();
                    return true;
                });

                argument.Arity = ArgumentArity.ZeroOrMore;

                argument.Parse("1 2 3")
                        .FindResultFor(argument)
                        .GetValueOrDefault()
                        .Should()
                        .Be(6);
            }
        }

        protected override Symbol CreateSymbol(string name)
        {
            return new Argument(name);
        }
    }
}