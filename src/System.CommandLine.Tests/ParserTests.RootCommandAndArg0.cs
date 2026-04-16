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
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    new Option<string>("-x")
                }
            };

            var result1 = command.Parse(Split("inner -x hello"));
            var result2 = command.Parse(Split("outer inner -x hello"));

            result1.Diagram().Should().Be(result2.Diagram());
        }
        
        [Fact]
        public void When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command()
        {
            var command = new RootCommand
            {
                new Command("inner")
                {
                    new Option<string>("-x")
                }
            };

            command.Parse(Split("inner -x hello")).Errors.Should().BeEmpty();

            var parserResult = command.Parse(Split($"\"{RootCommand.ExecutablePath}\" inner -x hello"));
            parserResult
               .Errors
               .Should()
               .ContainSingle(e => e.Message == LocalizationResources.UnrecognizedCommandOrArgument(RootCommand.ExecutablePath));
        }

        [Fact]
        public void When_parsing_an_unsplit_string_a_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    new Option<string>("-x")
                }
            };

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void When_parsing_an_unsplit_string_then_input_a_full_path_to_an_executable_is_matched_by_the_root_command()
        {
            var command = new RootCommand
            {
                new Command("inner")
                {
                    new Option<string>("-x")
                }
            };

            var result2 = command.Parse($"\"{RootCommand.ExecutablePath}\" inner -x hello");

            result2.RootCommandResult.IdentifierToken.Value.Should().Be(RootCommand.ExecutablePath);
        }

        [Fact]
        public void When_parsing_an_unsplit_string_then_a_renamed_RootCommand_can_be_omitted_from_the_parsed_args()
        {
            var rootCommand = new Command("outer")
            {
                new Command("inner")
                {
                    new Option<string>("-x")
                }
            };

            var result1 = rootCommand.Parse("inner -x hello");
            var result2 = rootCommand.Parse("outer inner -x hello");
            var result3 = rootCommand.Parse($"{RootCommand.ExecutableName} inner -x hello");

            result2.RootCommandResult.Command.Should().BeSameAs(result1.RootCommandResult.Command);
            result3.RootCommandResult.Command.Should().BeSameAs(result1.RootCommandResult.Command);
        }

        [Fact]
        public void When_parsing_a_string_array_option_values_containing_command_name_after_slash_are_not_treated_as_root_command()
        {
            // Path.GetFileName("/p:Key=something/myapp") returns "myapp",
            // which should not be matched as the root command name.
            var option = new Option<string>("--property", "/p") { Arity = ArgumentArity.ExactlyOne };
            var command = new Command("myapp");
            command.Options.Add(option);

            var result = command.Parse(Split("/p:Key=something/myapp"));

            result.Errors.Should().BeEmpty();
            result.GetResult(option).Should().NotBeNull();
        }

        [Theory]
        [InlineData("/p:DockerImage=registry.example.com/project/dotnet")]
        [InlineData("-p:DockerImage=registry.example.com/project/dotnet")]
        [InlineData("--property:DockerImage=registry.example.com/project/dotnet")]
        public void When_parsing_a_string_array_option_values_ending_with_slash_command_name_are_preserved(string arg)
        {
            // Regression test: Path.GetFileName extracts the last segment after '/',
            // so option values like ".../dotnet" falsely matched a command named "dotnet".
            var option = new Option<string>("--property", "/p", "-p") { Arity = ArgumentArity.ExactlyOne };
            var command = new Command("dotnet");
            command.Options.Add(option);

            var result = command.Parse(Split(arg));

            result.Errors.Should().BeEmpty();
            result.GetResult(option).Should().NotBeNull();
        }

        string[] Split(string value) => CommandLineParser.SplitCommandLine(value).ToArray();
    }
}