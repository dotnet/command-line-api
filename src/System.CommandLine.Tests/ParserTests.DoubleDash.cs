// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class DoubleDash
        {
            [Fact]
            public void When_legacy_behavior_is_enabled_then_the_portion_of_the_command_line_following_a_double_dasare_treated_as_unparsed_tokens()
            {
                var result = new CommandLineBuilder(new RootCommand { new Option("-o") })
                             .EnableLegacyDoubleDashBehavior()
                             .Build()
                             .Parse("-o \"some stuff\" -- x y z");

                result.UnparsedTokens
                      .Should()
                      .BeEquivalentSequenceTo("x", "y", "z");
            }

            [Fact]
            public void When_legacy_behavior_is_enabled_then_a_double_dash_specifies_that_tokens_matching_options_will_be_treated_as_unparsed_tokens()
            {
                var optionO = new Option(new[] { "-o" });
                var optionX = new Option(new[] { "-x" });
                var optionY = new Option(new[] { "-y" });
                var optionZ = new Option(new[] { "-z" });
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
            public void When_legacy_behavior_is_disabled_then_a_double_dash_specifies_that_further_command_line_args_will_be_treated_as_arguments()
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
            public void When_legacy_behavior_is_disabled_then_a_second_double_dash_is_parsed_as_an_argument()
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
    }
}