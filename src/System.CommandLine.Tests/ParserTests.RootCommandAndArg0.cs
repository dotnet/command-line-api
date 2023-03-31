// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public partial class ParserTests
{
    public partial class RootCommandAndArg0
    {
        [Fact]
        public void When_parsing_a_string_array_a_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("-x")
                }
            };

            var result1 = command.Parse(Split("inner -x hello"));
            var result2 = command.Parse(Split("outer inner -x hello"));

            result1.Diagram().Should().Be(result2.Diagram());
        }
        
        [Fact]
        public void When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command()
        {
            var command = new CliRootCommand
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("-x")
                }
            };

            command.Parse(Split("inner -x hello")).Errors.Should().BeEmpty();

            var parserResult = command.Parse(Split($"\"{CliRootCommand.ExecutablePath}\" inner -x hello"));
            parserResult
               .Errors
               .Should()
               .ContainSingle(e => e.Message == LocalizationResources.UnrecognizedCommandOrArgument(CliRootCommand.ExecutablePath));
        }

        [Fact]
        public void When_parsing_an_unsplit_string_a_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("-x")
                }
            };

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void When_parsing_an_unsplit_string_then_input_a_full_path_to_an_executable_is_matched_by_the_root_command()
        {
            var command = new CliRootCommand
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("-x")
                }
            };

            var result2 = command.Parse($"\"{CliRootCommand.ExecutablePath}\" inner -x hello");

            result2.RootCommandResult.IdentifierToken.Value.Should().Be(CliRootCommand.ExecutablePath);
        }

        [Fact]
        public void When_parsing_an_unsplit_string_then_a_renamed_RootCommand_can_be_omitted_from_the_parsed_args()
        {
            var rootCommand = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("-x")
                }
            };

            var result1 = rootCommand.Parse("inner -x hello");
            var result2 = rootCommand.Parse("outer inner -x hello");
            var result3 = rootCommand.Parse($"{CliRootCommand.ExecutableName} inner -x hello");

            result2.RootCommandResult.Command.Should().BeSameAs(result1.RootCommandResult.Command);
            result3.RootCommandResult.Command.Should().BeSameAs(result1.RootCommandResult.Command);
        }

        string[] Split(string value) => CliParser.SplitCommandLine(value).ToArray();
    }
}