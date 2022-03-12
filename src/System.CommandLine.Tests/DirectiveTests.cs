// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class DirectiveTests
    {
        [Fact]
        public void Directives_should_not_be_considered_as_unmatched_tokens()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse($"{RootCommand.ExecutableName} [parse] -y");

            result.UnmatchedTokens.Should().BeEmpty();
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.Contains("parse").Should().BeTrue();
            result.Tokens.Should().Contain(t => t.Value == "[parse]");
        }

        [Fact]
        public void Directives_should_parse_into_the_directives_collection()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.Contains("parse").Should().BeTrue();
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse("[parse] [suggest] -y");

            result.Directives.Contains("parse").Should().BeTrue();
            result.Directives.Contains("suggest").Should().BeTrue();
        }

        [Fact]
        public void Directives_must_be_the_first_argument()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse("-y [suggest]");

            result.Directives.Should().BeEmpty();
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
            var option = new Option<bool>("-y");

            var result = option.Parse($"{directive} -y");

            result.Directives.TryGetValues(expectedKey, out var values).Should().BeTrue();
            values.Should().BeEquivalentTo(expectedValue);
        }

        [Fact]
        public void Directives_without_a_value_specified_have_a_value_of_empty_string()
        {
            var option = new Option<bool>("-y");

            var result = option.Parse("[parse] -y");

            result.Directives.TryGetValues("parse", out var values).Should().BeTrue();
            values.Should().BeEmpty();
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[:value]")]
        public void Directives_must_have_a_non_empty_key(string directive)
        {
            var option = new Option<bool>("-a");

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
            var option = new Option<bool>("-a");

            var result = option.Parse($"{value} -a");

            result.Directives.Should().BeEmpty();
        }

        [Fact]
        public void When_a_directive_is_specified_more_than_once_then_its_values_are_aggregated()
        {
            var option = new Option<bool>("-a");

            var result = option.Parse("[directive:one] [directive:two] -a");

            result.Directives.TryGetValues("directive", out var values).Should().BeTrue();
            values.Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Directive_count_is_based_on_distinct_instances_of_directive_name()
        {
            var command = new RootCommand();

            var result = command.Parse("[one] [two] [one:a] [one:b]");

            result.Directives.Should().HaveCount(2);
        }

        [Fact]
        public void Directives_can_be_disabled()
        {
            var parser = new Parser(
                new CommandLineConfiguration(
                    new RootCommand
                    {
                        new Argument<List<string>>()
                    },
                    enableDirectives: false));

            var result = parser.Parse("[hello]");

            result.Directives.Count().Should().Be(0);
            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("[hello]");
        }
    }
}
