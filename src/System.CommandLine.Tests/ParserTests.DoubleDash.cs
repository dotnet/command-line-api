// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
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
                var option = new Option<string[]>(new[] { "-o", "--one" });
                var argument = new Argument<string[]>();
                var rootCommand = new RootCommand
                {
                    option,
                    argument
                };

                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior(false)
                             .Build()
                             .Parse("-o \"some stuff\" -- -o --one -x -y -z -o:foo");

                result.HasOption(option).Should().BeTrue();

                result.GetValueForOption(option).Should().BeEquivalentTo("some stuff");

                result.GetValueForArgument(argument).Should().BeEquivalentSequenceTo("-o", "--one", "-x", "-y", "-z", "-o:foo");

                result.UnparsedTokens.Should().BeEmpty();
            }

            [Fact]
            public void Unparsed_tokens_is_empty()
            {
                var option = new Option<string[]>(new[] { "-o", "--one" });
                var argument = new Argument<string[]>();
                var rootCommand = new RootCommand
                {
                    option,
                    argument
                };

                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior(false)
                             .Build()
                             .Parse("-o \"some stuff\" -- --one -x -y -z -o:foo");

                result.UnparsedTokens.Should().BeEmpty();
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1631
            public void No_errors_are_generated()
            {
                var option = new Option<string[]>(new[] { "-o", "--one" });
                var argument = new Argument<string[]>();
                var rootCommand = new RootCommand
                {
                    option,
                    argument
                };

                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior(false)
                             .Build()
                             .Parse("-o \"some stuff\" -- -o --one -x -y -z -o:foo");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void A_second_double_dash_is_parsed_as_an_argument()
            {
                var argument = new Argument<string[]>();
                var rootCommand = new RootCommand
                {
                    argument
                };

                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior(false)
                             .Build()
                             .Parse("a b c -- -- d");

                var strings = result.GetValueForArgument(argument);

                strings.Should().BeEquivalentSequenceTo("a", "b", "c", "--", "d");
            }
        }

        public class LegacyDoubleDashBehavior
        {
            [Fact]
            public void The_portion_of_the_command_line_following_a_double_is_treated_as_unparsed_tokens()
            {
                var result = new CommandLineBuilder(new RootCommand { new Option<string>("-o") })
                             .EnableLegacyDoubleDashBehavior()
                             .Build()
                             .Parse("-o \"some stuff\" -- x y z");

                result.UnparsedTokens
                      .Should()
                      .BeEquivalentSequenceTo("x", "y", "z");
            }

            [Fact]
            public void Subsequent_tokens_matching_options_will_be_treated_as_unparsed_tokens()
            {
                var optionO = new Option<string>(new[] { "-o" });
                var optionX = new Option<bool>(new[] { "-x" });
                var optionY = new Option<bool>(new[] { "-y" });
                var optionZ = new Option<bool>(new[] { "-z" });
                var rootCommand = new RootCommand
                {
                    optionO,
                    optionX,
                    optionY,
                    optionZ
                };
                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior()
                             .Build()
                             .Parse("-o \"some stuff\" -- -x -y -z -o:foo");

                result.HasOption(optionO).Should().BeTrue();
                result.HasOption(optionX).Should().BeFalse();
                result.HasOption(optionY).Should().BeFalse();
                result.HasOption(optionZ).Should().BeFalse();

                result.UnparsedTokens
                      .Should()
                      .BeEquivalentSequenceTo("-x",
                                              "-y",
                                              "-z",
                                              "-o:foo");
            }

            [Fact]
            public void Subsequent_tokens_matching_argument_will_be_treated_as_unparsed_tokens()
            {
                var argument = new Argument<int[]>();
                var rootCommand = new RootCommand
                {
                    argument
                };
                var result = new CommandLineBuilder(rootCommand)
                             .EnableLegacyDoubleDashBehavior()
                             .Build()
                             .Parse("1 2 3 -- 4 5 6 7");

                result.GetValueForArgument(argument).Should().BeEquivalentSequenceTo(1, 2, 3);
            }
        }
    }
}