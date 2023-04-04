﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests
{
    public class DirectiveTests
    {
        [Fact]
        public void Directives_should_be_considered_as_unmatched_tokens_when_they_are_not_matched()
        {
            CliDirective directive = new("parse");

            ParseResult result = Parse(new CliOption<bool>("-y"), directive, $"{CliRootCommand.ExecutableName} [nonExisting] -y");

            result.UnmatchedTokens.Should().ContainSingle("[nonExisting]");
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            CliDirective directive = new ("parse");

            ParseResult result = Parse(new CliOption<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Should().NotBeNull();
            result.Tokens.Should().Contain(t => t.Value == "[parse]");
        }

        [Fact]
        public void Directives_must_precede_other_symbols()
        {
            CliDirective directive = new("parse");

            ParseResult result = Parse(new CliOption<bool>("-y"), directive, "-y [parse]");

            result.FindResultFor(directive).Should().BeNull();
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            CliRootCommand root = new() { new CliOption<bool>("-y") };
            CliDirective parseDirective = new ("parse");
            CliDirective suggestDirective = new ("suggest");
            CliConfiguration config = new(root);
            config.Directives.Add(parseDirective);
            config.Directives.Add(suggestDirective);

            var result = root.Parse("[parse] [suggest] -y", config);

            result.FindResultFor(parseDirective).Should().NotBeNull();
            result.FindResultFor(suggestDirective).Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Multiple_instances_of_the_same_directive_can_be_invoked(bool invokeAsync)
        {
            var commandActionWasCalled = false;
            var directiveCallCount = 0;

            var testDirective = new TestDirective("test")
            {
                Action = new NonexclusiveTestAction(_ => directiveCallCount++)
            };

            var config = new CliConfiguration(new CliRootCommand
            {
                Action = new NonexclusiveTestAction(_ => commandActionWasCalled = true)
            })
            {
                Directives = { testDirective }
            };

            if (invokeAsync)
            {
                await config.InvokeAsync("[test:1] [test:2]");
            }
            else
            {
                config.Invoke("[test:1] [test:2]");
            }

            using var _ = new AssertionScope();

            commandActionWasCalled.Should().BeTrue();
            directiveCallCount.Should().Be(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Multiple_different_directives_can_be_invoked(bool invokeAsync)
        {
            bool commandActionWasCalled = false;
            bool directiveOneActionWasCalled = false;
            bool directiveTwoActionWasCalled = false;

            var directiveOne = new TestDirective("one")
            {
                Action = new NonexclusiveTestAction(_ => directiveOneActionWasCalled = true)
            };
            var directiveTwo = new TestDirective("two")
            {
                Action = new NonexclusiveTestAction(_ => directiveTwoActionWasCalled = true)
            };
            var config = new CliConfiguration(new CliRootCommand
            {
                Action = new NonexclusiveTestAction(_ => commandActionWasCalled = true)
            })
            {
                Directives = { directiveOne, directiveTwo }
            };

            if (invokeAsync)
            {
                await config.InvokeAsync("[one] [two]");
            }
            else
            {
                config.Invoke("[one] [two]");
            }

            using var _ = new AssertionScope();

            commandActionWasCalled.Should().BeTrue();
            directiveOneActionWasCalled.Should().BeTrue();
            directiveTwoActionWasCalled.Should().BeTrue();
        }

        public class TestDirective : CliDirective
        {
            public TestDirective(string name) : base(name)
            {
            }
        }

        private class NonexclusiveTestAction : CliAction
        {
            private readonly Action<ParseResult> _invoke;

            public NonexclusiveTestAction(Action<ParseResult> invoke)
            {
                _invoke = invoke;
                Exclusive = false;
            }

            public override int Invoke(ParseResult parseResult)
            {
                _invoke(parseResult);
                return 0;
            }

            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ;
                return Task.FromResult(Invoke(parseResult));
            }
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
            CliDirective directive = new(key);

            ParseResult result = Parse(new CliOption<bool>("-y"), directive, $"{wholeText} -y");

            result.FindResultFor(directive).Values.Single().Should().Be(expectedValue);
        }

        [Fact]
        public void Directives_without_a_value_specified_have_no_values()
        {
            CliDirective directive = new("parse");

            ParseResult result = Parse(new CliOption<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Values.Should().BeEmpty();
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[:value]")]
        public void Directives_must_have_a_non_empty_key(string directive)
        {
            CliOption<bool> option = new ("-a");
            CliRootCommand root = new () { option };

            var result = root.Parse($"{directive} -a");

            result.UnmatchedTokens.Should().Contain(directive);
        }

        [Theory]
        [InlineData("[par se]", "[par", "se]")]
        [InlineData("[ parse]", "[", "parse]")]
        [InlineData("[parse ]", "[parse", "]")]
        public void Directives_cannot_contain_spaces(string value, string firstUnmatchedToken, string secondUnmatchedToken)
        {
            Action create = () => new CliDirective(value);
            create.Should().Throw<ArgumentException>();

            CliDirective directive = new("parse");
            ParseResult result = Parse(new CliOption<bool>("-y"), directive, $"{value} -y");
            result.FindResultFor(directive).Should().BeNull();

            result.UnmatchedTokens.Should().BeEquivalentTo(firstUnmatchedToken, secondUnmatchedToken);
        }

        [Fact]
        public void When_a_directive_is_specified_more_than_once_then_its_values_are_aggregated()
        {
            CliDirective directive = new("directive");

            ParseResult result = Parse(new CliOption<bool>("-a"), directive, "[directive:one] [directive:two] -a");

            result.FindResultFor(directive).Values.Should().BeEquivalentTo("one", "two");
        }

        private static ParseResult Parse(CliOption option, CliDirective directive, string commandLine)
        {
            CliRootCommand root = new() { option };
            CliConfiguration config = new(root);
            config.Directives.Add(directive);

            return root.Parse(commandLine, config);
        }
    }
}