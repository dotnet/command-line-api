// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class ArgumentParserTests
    {
        [Fact]
        public void The_failure_message_returned_is_the_first_to_fail()
        {
            var parser = new ArgumentParser<string>(parsedSymbol => ArgumentParseResult.Success(""));

            parser.AddValidator((value, parsedSymbol) => ArgumentParseResult.Failure("first error"));
            parser.AddValidator((value, parsedSymbol) => ArgumentParseResult.Failure("second error"));

            var builder = new ArgumentRuleBuilder<string> { ArgumentParser = parser };
             
            var result = Create.Option("-x", "", builder.Build()).Parse("-x")["x"].Result;

            result.Should().BeOfType<FailedArgumentParseResult>().Which.ErrorMessage.Should().Be("first error");
        }

        [Fact]
        public void Later_rules_are_not_evaluated()
        {
            var parser = new ArgumentParser<string>(parsedSymbol => ArgumentParseResult.Success(""));

            var firstRuleWasCalled = false;
            var secondRuleWasCalled = false;

            parser.AddValidator((value, parsedSymbol) =>
            {
                firstRuleWasCalled = true;
                return ArgumentParseResult.Failure("first error");
            });
            parser.AddValidator((value, parsedSymbol) =>
            {
                secondRuleWasCalled = true;
                return ArgumentParseResult.Failure("second error");
            });

            var builder = new ArgumentRuleBuilder<string> { ArgumentParser = parser };

            Create.Option("-x", "", builder.Build()).Parse("-x");

            firstRuleWasCalled.Should().BeTrue();
            secondRuleWasCalled.Should().BeFalse();
        }
    }
}
