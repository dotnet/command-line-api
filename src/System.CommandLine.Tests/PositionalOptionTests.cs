﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class PositionalOptionTests
    {
        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_overriden()
        {
            var configuration = new CommandLineConfiguration(
                new[]
                {
                    new Option(
                        "-x",
                        "",
                        new Argument<int>(123)),
                    new Option(
                        "-y",
                        "",
                        new Argument<int>(456))
                },
                enablePositionalOptions: true);

            var parser = new Parser(configuration);

            var result = parser.Parse("42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(456);
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_a_given_positional_value_should_override_with_other_specified()
        {
            var configuration = new CommandLineConfiguration(
                new[]
                {
                    new Option(
                        "-x",
                        "",
                        new Argument<int>(123)),
                    new Option(
                        "-y",
                        "",
                        new Argument<int>(456))
                },
                enablePositionalOptions: true);

            var parser = new Parser(configuration);

            var result = parser.Parse("-y 23 42");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(42);
            result.RootCommandResult.ValueForOption("-y").Should().Be(23);
        }

        [Fact]
        public void When_a_sibling_commands_have_options_with_the_same_name_it_matches_based_on_command()
        {
            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand(
                             new Command("command1")
                             {
                                 new Option("-anon", "", new Argument<string>())
                             }
                         )
                         .AddCommand(
                             new Command("command2")
                             {
                                 new Option("-anon", "", new Argument<string>())
                             }
                         )
                         .Build();

            ParseResult result = parser.Parse("command2 anon-value");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Name.Should().Be("command2");
            result.CommandResult["-anon"].GetValueOrDefault<string>().Should().Be("anon-value");
        }

        [Theory]
        [InlineData(2, 0)]
        [InlineData(1, 1)]
        [InlineData(0, 2)]
        public void When_nested_subcommands_have_options_they_can_be_positional(
            int subcommand1Options,
            int subcommand2Options)
        {
            var subcommand1 = new Command("subcommand1");
            foreach (int optionIndex in Enumerable.Range(1, subcommand1Options))
            {
                subcommand1.AddOption(new Option($"-anon{optionIndex}", "", new Argument<string>()));
            }

            var subcommand2 = new Command("subcommand2");
            subcommand1.AddCommand(subcommand2);
            foreach (int optionIndex in Enumerable.Range(1, subcommand2Options))
            {
                subcommand2.AddOption(new Option($"-anon{optionIndex}", "", new Argument<string>()));
            }

            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand(subcommand1)
                         .Build();

            string commandLine = string.Join(" ", GetCommandLineParts());

            ParseResult result = parser.Parse(commandLine);

            result.Errors.Should().BeEmpty();
            for (var commandResult = result.CommandResult; commandResult != null; commandResult = commandResult.Parent)
            {
                int index = 1;
                foreach (var optionResult in commandResult.Children.OfType<OptionResult>())
                {
                    optionResult.GetValueOrDefault<string>().Should().Be($"anon{index++}-value");
                }
            }

            IEnumerable<string> GetCommandLineParts()
            {
                yield return "subcommand1";
                foreach (int optionIndex in Enumerable.Range(1, subcommand1Options))
                {
                    yield return $"anon{optionIndex}-value";
                }

                yield return "subcommand2";
                foreach (int optionIndex in Enumerable.Range(1, subcommand2Options))
                {
                    yield return $"anon{optionIndex}-value";
                }
            }
        }

        [Fact]
        public void When_a_subcommand_has_options_they_can_be_positional()
        {
            var parser = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddCommand(new Command("subcommand")
                                     {
                                         new Option("-anon1", "", new Argument<string>()),
                                         new Option("-anon2", "", new Argument<string>())
                                     }
                         )
                         .Build();

            ParseResult result = parser.Parse("subcommand anon1-value anon2-value");

            result.Errors.Should().BeEmpty();
            result.CommandResult["-anon1"].GetValueOrDefault<string>().Should().Be("anon1-value");
            result.CommandResult["-anon2"].GetValueOrDefault<string>().Should().Be("anon2-value");
        }

        [Fact]
        public void When_option_argument_is_provided_without_option_name_then_argument_position_is_assumed()
        {
            var result = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddOption(new Option("-a", "", new Argument<string>()))
                         .Build()
                         .Parse("value-for-a");

            result.ValueForOption("-a").Should().Be("value-for-a");
        }

        [Fact]
        public void When_multiple_option_arguments_are_provided_without_option_name_then_argument_positions_are_assumed()
        {
            var rootCommand = new RootCommand
                              {
                                  new Option("-a", "", new Argument<string>()),
                                  new Option("-b"),
                                  new Option("-c", "", new Argument<string>())
                              };
            var result = new CommandLineBuilder(rootCommand)
                         .EnablePositionalOptions()
                         .Build()
                         .Parse("value-for-a value-for-c");

            result.ValueForOption("-a").Should().Be("value-for-a");
            result.ValueForOption("-c").Should().Be("value-for-c");
            result.HasOption("-b").Should().BeFalse();
        }

        [Fact]
        public void When_multiple_option_arguments_are_provided_with_first_option_name_then_argument_positions_are_assumed()
        {
            var rootCommand = new RootCommand
                              {
                                  new Option("-a", "", new Argument<string>()),
                                  new Option("-b"),
                                  new Option("-c", "", new Argument<string>())
                              };
            var result = new CommandLineBuilder(rootCommand)
                         .EnablePositionalOptions()
                         .Build()
                         .Parse("-a value-for-a value-for-c");

            result.ValueForOption("-a").Should().Be("value-for-a");
            result.ValueForOption("-c").Should().Be("value-for-c");
            result.HasOption("-b").Should().BeFalse();
        }

        [Fact]
        public void When_multiple_option_arguments_are_provided_with_second_option_is_first_positions_are_assumed()
        {
            var result = new CommandLineBuilder()
                         .EnablePositionalOptions()
                         .AddOption(new Option("-a", "", new Argument<string>()))
                         .AddOption(new Option("-b"))
                         .AddOption(new Option("-c", "", new Argument<string>()))
                         .Build()
                         .Parse("-c value-for-c value-for-a");

            result.ValueForOption("-a").Should().Be("value-for-a");
            result.ValueForOption("-c").Should().Be("value-for-c");
            result.HasOption("-b").Should().BeFalse();
        }
    }
}
