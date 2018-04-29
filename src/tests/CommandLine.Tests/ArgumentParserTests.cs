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
            var parser = new ArgumentParser<string>(parsedSymbol => Result.Success(""));

            parser.AddValidator((value, parsedSymbol) => Result.Failure("first error"));
            parser.AddValidator((value, parsedSymbol) => Result.Failure("second error"));

            var builder = new ArgumentRuleBuilder<string> { ArgumentParser = parser };

            var parsedOption = new ParsedOption(Create.Option("-x", "", builder.Build()));

            Result result = parsedOption.Result();
            result.Should().BeOfType<FailedResult>().Which.Error.Should().Be("first error");
        }

        [Fact]
        public void Later_rules_are_not_evaluated()
        {
            var parser = new ArgumentParser<string>(parsedSymbol => Result.Success(""));

            var secondRuleWasCalled = false;

            parser.AddValidator((value, parsedSymbol) => Result.Failure("first error"));
            parser.AddValidator((value, parsedSymbol) =>
            {
                secondRuleWasCalled = true;
                return Result.Failure("second error");
            });

            var builder = new ArgumentRuleBuilder<string> { ArgumentParser = parser };

            var parsedOption = new ParsedOption(Create.Option("-x", "", builder.Build()));

            parsedOption.Result();

            secondRuleWasCalled.Should().BeFalse();
        }

    }
}