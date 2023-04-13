// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class DefaultDoubleDashBehavior
        {
            [Fact] // https://github.com/dotnet/command-line-api/issues/1238
            public void Subsequent_tokens_are_parsed_as_arguments_even_if_they_match_option_identifiers()
            {
                var option = new CliOption<string[]>("-o", "--one");
                var argument = new CliArgument<string[]>("arg");
                var rootCommand = new CliRootCommand
                {
                    option,
                    argument
                };

                var result = rootCommand.Parse("-o \"some stuff\" -- -o --one -x -y -z -o:foo");

                result.GetResult(option).Should().NotBeNull();

                result.GetValue(option).Should().BeEquivalentTo("some stuff");

                result.GetValue(argument).Should().BeEquivalentSequenceTo("-o", "--one", "-x", "-y", "-z", "-o:foo");

                result.UnmatchedTokens.Should().BeEmpty();
            }

            [Fact]
            public void Unmatched_tokens_is_empty()
            {
                var option = new CliOption<string[]>("-o", "--one");
                var argument = new CliArgument<string[]>("arg");
                var rootCommand = new CliRootCommand
                {
                    option,
                    argument
                };

                var result = rootCommand.Parse("-o \"some stuff\" -- --one -x -y -z -o:foo");

                result.UnmatchedTokens.Should().BeEmpty();
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1631
            public void No_errors_are_generated()
            {
                var option = new CliOption<string[]>("-o", "--one");
                var argument = new CliArgument<string[]>("arg");
                var rootCommand = new CliRootCommand
                {
                    option,
                    argument
                };

                var result = rootCommand.Parse("-o \"some stuff\" -- -o --one -x -y -z -o:foo");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void A_second_double_dash_is_parsed_as_an_argument()
            {
                var argument = new CliArgument<string[]>("arg");
                var rootCommand = new CliRootCommand
                {
                    argument
                };

                var result = rootCommand.Parse("a b c -- -- d");

                var strings = result.GetValue(argument);

                strings.Should().BeEquivalentSequenceTo("a", "b", "c", "--", "d");
            }
        }
    }
}