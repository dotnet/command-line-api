// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests;

public partial class ParserTests
{
    public partial class RootCommandAndArg0
    {
        [Fact]
        public void When_parsing_a_string_array_a_root_command_can_be_omitted_from_the_parsed_args()
        {
            var option = new Option<string>("-x");
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    option
                }
            };

            var result1 = command.Parse(Split("inner -x hello"));
            var result2 = command.Parse(Split("outer inner -x hello"));

            using var _ = new AssertionScope();

            result1.Diagram().Should().Be(result2.Diagram());

            foreach (var result in new[] { result1, result2 })
            {
                result.Errors.Should().BeEmpty();
                result.RootCommandResult.Command.Name.Should().Be("outer");
                result.CommandResult.Command.Name.Should().Be("inner");
                result.GetValue(option).Should().Be("hello");
            }
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

            var parserResult = command.Parse(Split($"\"{RootCommand.ExecutablePath}\" inner -x hello"));
            parserResult
               .Errors
               .Should()
               .ContainSingle(e => e.Message == LocalizationResources.UnrecognizedCommandOrArgument(RootCommand.ExecutablePath));
        }

        [Fact]
        public void When_parsing_an_unsplit_string_a_root_command_can_be_omitted_from_the_parsed_args()
        {
            var option = new Option<string>("-x");
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    option
                }
            };

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            using var _ = new AssertionScope();

            result1.Diagram().Should().Be(result2.Diagram());

            foreach (var result in new[] { result1, result2 })
            {
                result.Errors.Should().BeEmpty();
                result.RootCommandResult.Command.Name.Should().Be("outer");
                result.CommandResult.Command.Name.Should().Be("inner");
                result.GetValue(option).Should().Be("hello");
            }
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
        public void When_parsing_a_string_array_option_values_containing_command_name_after_slash_are_not_treated_as_root_command()
        {
            // Path.GetFileName("/p:Key=something/myapp") returns "myapp",
            // which should not be matched as the root command name.
            var option = new Option<string>("--property", "/p") { Arity = ArgumentArity.ExactlyOne };
            var command = new Command("myapp");
            command.Options.Add(option);

            var result = command.Parse(Split("/p:Key=something/myapp"));

            using var _ = new AssertionScope();

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

            using var _ = new AssertionScope();

            result.Errors.Should().BeEmpty();
            result.GetResult(option).Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(PathLikeFirstArgsMatchingTheRootCommand))]
        public void When_parsing_a_string_array_a_path_like_first_arg_matching_the_root_command_is_treated_as_the_root_command(string pathLikeRootArg)
        {
            var option = new Option<string>("-x");
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    option
                }
            };

            var result = command.Parse([pathLikeRootArg, "inner", "-x", "hello"]);

            using var _ = new AssertionScope();

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.IdentifierToken.Value.Should().Be(pathLikeRootArg);
            result.CommandResult.Command.Name.Should().Be("inner");
            result.GetValue(option).Should().Be("hello");
        }

        public static TheoryData<string> PathLikeFirstArgsMatchingTheRootCommand
        {
            get
            {
                var data = new TheoryData<string>
                {
                    "./outer",
                    "tools/outer"
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    data.Add(@".\outer");
                    data.Add(@"tools\outer");
                }

                return data;
            }
        }

        [Fact]
        public void RootCommand_matches_its_executable_name_as_the_first_arg_the_same_way_a_named_Command_matches_its_name()
        {
            // Guards the assumption that a named Command can stand in for a RootCommand
            // in arg0-matching tests: RootCommand.Name is forced to the executable name and
            // cannot be changed, so the equivalent scenario uses the executable name as arg0.
            var option = new Option<string>("-x");
            var rootCommand = new RootCommand
            {
                new Command("inner")
                {
                    option
                }
            };

            var result = rootCommand.Parse([RootCommand.ExecutableName, "inner", "-x", "hello"]);

            using var _ = new AssertionScope();

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.IdentifierToken.Value.Should().Be(RootCommand.ExecutableName);
            result.CommandResult.Command.Name.Should().Be("inner");
            result.GetValue(option).Should().Be("hello");
        }

#if NETFRAMEWORK
        [Fact]
        public void When_parsing_a_string_array_arguments_containing_invalid_path_characters_the_netframework_PathGetFileName_exception_is_ignored()
        {
            const string invalidPath = "my\0app";

            // On .NET Framework, this throws ArgumentException for illegal characters in path.
            Action getFileName = () => Path.GetFileName(invalidPath);

            var argument = new Argument<string>("value");
            var command = new Command("myapp") { argument };
            var result = command.Parse(invalidPath);

            using var _ = new AssertionScope();

            getFileName.Should().Throw<ArgumentException>();
            result.Errors.Should().BeEmpty();
            result.GetValue(argument).Should().Be(invalidPath);
        }
#endif

        string[] Split(string value) => CommandLineParser.SplitCommandLine(value).ToArray();
    }
}