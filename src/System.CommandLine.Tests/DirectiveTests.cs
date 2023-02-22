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
        public void Directives_should_not_be_considered_as_unmatched_tokens_when_they_are_enabled()
        {
            RootCommand root = new () { new Option<bool>("-y") };
            CommandLineBuilder builder = new (root);
            builder.Directives.Add(new ("some"));

            var result = builder.Build().Parse($"{RootCommand.ExecutableName} [parse] -y");

            result.UnmatchedTokens.Should().BeEmpty();
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            Directive directive = new ("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Should().NotBeNull();
            result.Tokens.Should().Contain(t => t.Value == "[parse]");
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            RootCommand root = new() { new Option<bool>("-y") };
            Directive parseDirective = new ("parse");
            Directive suggestDirective = new ("suggest");
            CommandLineBuilder builder = new(root);
            builder.Directives.Add(parseDirective);
            builder.Directives.Add(suggestDirective);

            var result = builder.Build().Parse("[parse] [suggest] -y");

            result.FindResultFor(parseDirective).Should().NotBeNull();
            result.FindResultFor(suggestDirective).Should().NotBeNull();
        }

        [Fact]
        public void Directives_must_be_the_first_argument()
        {
            Directive directive = new("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "-y [parse]");

            result.FindResultFor(directive).Should().BeNull();
        }

        [Theory]
        [InlineData("[key:value]", "key", "value")]
        [InlineData("[key:value:more]", "key", "value:more")]
        [InlineData("[key:]", "key", "")]
        public void Directives_can_have_a_value_which_is_everything_after_the_first_colon(
            string wholeText,
            string key,
            string expectedValue)
        {
            Directive directive = new(key);

            ParseResult result = Parse(new Option<bool>("-y"), directive, $"{wholeText} -y");

            result.FindResultFor(directive).Values.Single().Should().Be(expectedValue);
        }

        [Fact]
        public void Directives_without_a_value_specified_have_no_values()
        {
            Directive directive = new("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Values.Should().BeEmpty();
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[:value]")]
        public void Directives_must_have_a_non_empty_key(string directive)
        {
            var option = new Option<bool>("-a");

            var result = option.Parse($"{directive} -a");

            result.UnmatchedTokens.Should().Contain(directive);
        }

        [Theory]
        [InlineData("[par se]")]
        [InlineData("[ parse]")]
        [InlineData("[parse ]")]
        public void Directives_cannot_contain_spaces(string value)
        {
            Action create = () => new Directive(value);

            create.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_a_directive_is_specified_more_than_once_then_its_values_are_aggregated()
        {
            Directive directive = new("directive");

            ParseResult result = Parse(new Option<bool>("-a"), directive, "[directive:one] [directive:two] -a");

            result.FindResultFor(directive).Values.Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void When_directives_are_not_enabled_they_are_treated_as_regular_tokens()
        {
            var parser = new Parser(
                new CommandLineConfiguration(
                    new RootCommand
                    {
                        new Argument<List<string>>()
                    }));

            var result = parser.Parse("[hello]");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("[hello]");
        }

        private static ParseResult Parse(Option option, Directive directive, string commandLine)
        {
            RootCommand root = new() { option };
            CommandLineBuilder builder = new(root);
            builder.Directives.Add(directive);

            return builder.Build().Parse(commandLine);
        }
    }
}