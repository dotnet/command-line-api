// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ResponseFileTests : IDisposable
    {
        private readonly List<FileInfo> _responseFiles = new();

        public void Dispose()
        {
            foreach (var responseFile in _responseFiles)
            {
                responseFile.Delete();
            }
        }

        private string CreateResponseFile(params string[] lines)
        {
            var responseFile = new FileInfo(Path.GetTempFileName());

            using (var writer = new StreamWriter(responseFile.FullName))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }

            _responseFiles.Add(responseFile);

            return responseFile.FullName;
        }

        [Fact]
        public void When_response_file_specified_it_loads_options_from_response_file()
        {
            var option = new CliOption<bool>("--flag");

            var result = new CliRootCommand { option }.Parse($"@{CreateResponseFile("--flag")}");

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_options_with_arguments_from_response_file()
        {
            var responseFile = CreateResponseFile(
                "--flag",
                "--flag2",
                "123");

            var optionOne = new CliOption<bool>("--flag");

            var optionTwo = new CliOption<int>("--flag2");
            var result = new CliRootCommand
                         {
                             optionOne,
                             optionTwo
                         }
                .Parse($"@{responseFile}");

            result.GetResult(optionOne).Should().NotBeNull();
            result.GetValue(optionTwo).Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_command_arguments_from_response_file()
        {
            var responseFile = CreateResponseFile(
                "one",
                "two",
                "three");

            var result = new CliRootCommand
            {
                new CliArgument<string[]>("arg")
            }
            .Parse($"@{responseFile}");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentSequenceTo("one", "two", "three");
        }

        [Fact]
        public void Response_file_can_provide_subcommand_arguments()
        {
            var responseFile = CreateResponseFile(
                "one",
                "two",
                "three");

            var result = new CliRootCommand
                         {
                             new CliCommand("subcommand")
                             {
                                 new CliArgument<string[]>("arg")
                             }
                         }
                .Parse($"subcommand @{responseFile}");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentSequenceTo("one", "two", "three");
        }

        [Fact]
        public void Response_file_can_provide_subcommand()
        {
            var responseFile = CreateResponseFile("subcommand");

            var result = new CliRootCommand
                         {
                             new CliCommand("subcommand")
                             {
                                 new CliArgument<string[]>("arg")
                             }
                         }
                .Parse($"@{responseFile} one two three");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentSequenceTo("one", "two", "three");
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_subcommand_arguments_from_response_file()
        {
            var responseFile = CreateResponseFile(
                "one",
                "two",
                "three");

            var result = new CliRootCommand
                         {
                             new CliCommand("subcommand")
                             {
                                 new CliArgument<string[]>("arg")
                             }
                         }
                .Parse($"subcommand @{responseFile}");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentSequenceTo("one", "two", "three");
        }

        [Fact]
        public void Response_file_can_contain_blank_lines()
        {
            var responseFile = CreateResponseFile(
                "--flag",
                "",
                "123");

            var option = new CliOption<int>("--flag");

            var result = new CliRootCommand
                {
                    option
                }
                .Parse($"@{responseFile}");

            result.GetValue(option).Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Response_file_can_contain_comments_which_are_ignored_when_loaded()
        {
            var optionOne = new CliOption<bool>("--flag");
            var optionTwo = new CliOption<bool>("--flag2");

            var responseFile = CreateResponseFile(
                "# comment one",
                "--flag",
                "# comment two",
                "#",
                " # comment two",
                "--flag2");

            var result = new CliRootCommand
            {
                optionOne,
                optionTwo
            }.Parse($"@{responseFile}");

            result.GetResult(optionOne).Should().NotBeNull();
            result.GetResult(optionTwo).Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_does_not_exist_then_error_is_returned()
        {
            var optionOne = new CliOption<bool>("--flag");
            var optionTwo = new CliOption<bool>("--flag2");

            var result = new CliRootCommand
                         {
                             optionOne,
                             optionTwo
                         }.Parse("@nonexistent.rsp");

            result.GetResult(optionOne).Should().BeNull();
            result.GetResult(optionTwo).Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be("Response file not found 'nonexistent.rsp'.");
        }

        [Fact]
        public void When_response_filepath_is_not_specified_then_error_is_returned()
        {
            var optionOne = new CliOption<bool>("--flag");
            var optionTwo = new CliOption<bool>("--flag2");

            var result = new CliRootCommand
                         {
                             optionOne,
                             optionTwo
                         }
                .Parse("@");

            result.GetResult(optionOne).Should().BeNull();
            result.GetResult(optionTwo).Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors
                  .Single()
                  .Message
                  .Should()
                  .Be("Unrecognized command or argument '@'.");
        }

        [Fact]
        public void When_response_file_cannot_be_read_then_specified_error_is_returned()
        {
            var nonexistent = Path.GetTempFileName();
            var optionOne = new CliOption<bool>("--flag");
            var optionTwo = new CliOption<bool>("--flag2");

            using (File.Open(nonexistent, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = new CliRootCommand
                             {
                                 optionOne,
                                 optionTwo
                             }.Parse($"@{nonexistent}");

                result.GetResult(optionOne).Should().BeNull();
                result.GetResult(optionTwo).Should().BeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.Single().Message.Should().StartWith($"Error reading response file '{nonexistent}'");
            }
        }

        [Theory]
        [InlineData("--flag \"first value\" --flag2 123")]
        [InlineData("--flag:\"first value\" --flag2:123")]
        [InlineData("--flag=\"first value\" --flag2=123")]
        public void When_response_file_parse_as_space_separated_returns_expected_values(string input)
        {
            var responseFile = CreateResponseFile(input);

            var optionOne = new CliOption<string>("--flag");
            var optionTwo = new CliOption<int>("--flag2");

            var rootCommand = new CliRootCommand
            {
                optionOne,
                optionTwo
            };
            CliConfiguration config = new (rootCommand);

            var result = rootCommand.Parse($"@{responseFile}", config);

            result.GetValue(optionOne).Should().Be("first value");
            result.GetValue(optionTwo).Should().Be(123);
        }

        [Fact]
        public void When_response_file_processing_is_disabled_then_it_returns_response_file_name_as_argument()
        {
            CliRootCommand command = new ()
            {
                new CliArgument<List<string>>("arg")
            };
            CliConfiguration configuration = new(command)
            {
                ResponseFileTokenReplacer = null
            };

            var result = CliParser.Parse(command, "@file.rsp", configuration);

            result.Tokens
                  .Should()
                  .Contain(t => t.Value == "@file.rsp" && 
                                t.Type == CliTokenType.Argument);
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void Response_files_can_refer_to_other_response_files()
        {
            var file3 = CreateResponseFile("--three", "3");
            var file2 = CreateResponseFile($"@{file3}", "--two", "2");
            var file1 = CreateResponseFile("--one", "1", $"@{file2}");

            var option1 = new CliOption<int>("--one");
            var option2 = new CliOption<int>("--two");
            var option3 = new CliOption<int>("--three");

            var command = new CliRootCommand
                          {
                              option1,
                              option2,
                              option3
                          };

            var result = command.Parse($"@{file1}");

            result.GetValue(option1).Should().Be(1);
            result.GetValue(option1).Should().Be(1);
            result.GetValue(option2).Should().Be(2);
            result.GetValue(option3).Should().Be(3);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_trailing_spaces_they_are_ignored()
        {
            var responseFile = CreateResponseFile("--option1 ", "value1 ", "--option2\t", "2\t");

            var option1 = new CliOption<string>("--option1");
            var option2 = new CliOption<int>("--option2");

            var result = new CliRootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_leading_spaces_they_are_ignored()
        {
            var responseFile = CreateResponseFile(" --option1", " value1", "\t--option2", "\t2");

            var option1 = new CliOption<string>("--option1");
            var option2 = new CliOption<int>("--option2");

            var result = new CliRootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_trailing_and_leading_spaces_they_are_ignored()
        {
            var responseFile = CreateResponseFile(" --option1 ", " value1 ", "\t--option2\t", "\t2\t");

            var option1 = new CliOption<string>("--option1");
            var option2 = new CliOption<int>("--option2");

            var result = new CliRootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
            result.Errors.Should().BeEmpty();
        }
    }
}
