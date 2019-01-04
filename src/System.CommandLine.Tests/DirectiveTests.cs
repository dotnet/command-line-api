// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class DirectiveTests
    {
        [Fact]
        public void Directives_should_not_be_considered_as_unmatched_tokens()
        {
            var option = new Option("-y");

            var result = option.Parse($"{RootCommand.ExeName} [parse] -y");

            result.UnmatchedTokens.Should().BeEmpty();
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            var option = new Option("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.Keys.Should().Contain("parse");
            result.Tokens.Should().Contain("[parse]");
        }

        [Fact]
        public void Directives_should_parse_into_the_directives_collection()
        {
            var option = new Option("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.Keys.Should().Contain("parse");
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            var option = new Option("-y");

            var result = option.Parse("[parse] [suggest] -y");

            result.Directives.Keys.Should().Contain("parse");
            result.Directives.Keys.Should().Contain("suggest");
        }

        [Fact]
        public void Directives_must_be_the_first_argument()
        {
            var option = new Option("-y");

            var result = option.Parse("-y [suggest]");

            result.UnmatchedTokens.Should().Contain("[suggest]");
        }

        [Theory]
        [InlineData("[key:value]", "key", "value")]
        [InlineData("[key:value:more]", "key", "value:more")]
        [InlineData("[key:]", "key", "")]
        public void Directives_can_have_a_value_which_is_everything_after_the_first_colon(
            string directive,
            string expectedKey,
            string expectedValue)
        {
            var option = new Option("-y");

            var result = option.Parse($"{directive} -y");

            result.Directives
                  .Should()
                  .Contain(new KeyValuePair<string, string>(expectedKey, expectedValue));
        }

        [Fact]
        public void Directives_without_a_value_specified_have_a_value_of_empty_string()
        {
            var option = new Option("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.Should().Contain(new KeyValuePair<string, string>("parse", ""));
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[:value]")]
        public void Directives_must_have_a_non_empty_key(string directive)
        {
            var option = new Option("-a");

            var result = option.Parse($"{directive} -a");

            result.Directives.Should().BeEmpty();
            result.UnmatchedTokens.Should().Contain(directive);
        }

        [Theory]
        [InlineData("[par se]")]
        [InlineData("[ parse]")]
        [InlineData("[parse ]")]
        public void Directives_cannot_contain_spaces(object value)
        {
            var option = new Option("-a");

            var result = option.Parse($"{value} -a");

            result.Directives.Should().BeEmpty();
        }

        [Fact]
        public void When_a_directive_is_specified_more_than_once_then_its_value_is_overwritten()
        {
            var option = new Option("-a");

            var result = option.Parse("[directive:one] [directive:two] -a");

            result.Directives["directive"].Should().Be("two");
        }
    }
}
