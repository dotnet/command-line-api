// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class ArgumentsRuleCompositionTests
    {
        [Fact]
        public void Composed_rule_results_are_equivalent_when_one_is_successful_and_the_other_fails()
        {
            var fail = new ArgumentsRule(o => "fail");
            var succeed = new ArgumentsRule(o => "fail");

            var appliedOption = new AppliedOption(Create.Option("-x", ""));

            fail.And(succeed).Validate(appliedOption).Should().Be("fail");
            succeed.And(fail).Validate(appliedOption).Should().Be("fail");
        }

        [Fact]
        public void The_failure_message_returned_is_the_first_to_fail()
        {
            var first = new ArgumentsRule(o => "first error");
            var second = new ArgumentsRule(o => "second error");

            var appliedOption = new AppliedOption(Create.Option("-x", ""));

            first.And(second).Validate(appliedOption).Should().Be("first error");
        }

        [Fact]
        public void Later_rules_are_not_evaluated()
        {
            var secondRuleWasCalled = false;
            var first = new ArgumentsRule(o => "first error");
            var second = new ArgumentsRule(o =>
            {
                secondRuleWasCalled = true;
                return "second error";
            });

            var appliedOption = new AppliedOption(Create.Option("-x", ""));

            first.And(second).Validate(appliedOption);

            secondRuleWasCalled.Should().BeFalse();
        }

    }
}