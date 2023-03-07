﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            var option = new Option<bool>("--flag");

            var result = new RootCommand { option }.Parse($"@{CreateResponseFile("--flag")}");

            result.FindResultFor(option).Should().NotBeNull();
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_options_with_arguments_from_response_file()
        {
            var responseFile = CreateResponseFile(
                "--flag",
                "--flag2",
                "123");

            var optionOne = new Option<bool>("--flag");

            var optionTwo = new Option<int>("--flag2");
            var result = new RootCommand
                         {
                             optionOne,
                             optionTwo
                         }
                .Parse($"@{responseFile}");

            result.FindResultFor(optionOne).Should().NotBeNull();
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

            var result = new RootCommand
            {
                new Argument<string[]>("arg")
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

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>("arg")
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

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>("arg")
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

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>("arg")
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

            var option = new Option<int>("--flag");

            var result = new RootCommand
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
            var optionOne = new Option<bool>("--flag");
            var optionTwo = new Option<bool>("--flag2");

            var responseFile = CreateResponseFile(
                "# comment one",
                "--flag",
                "# comment two",
                "#",
                " # comment two",
                "--flag2");

            var result = new RootCommand
            {
                optionOne,
                optionTwo
            }.Parse($"@{responseFile}");

            result.FindResultFor(optionOne).Should().NotBeNull();
            result.FindResultFor(optionTwo).Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_does_not_exist_then_error_is_returned()
        {
            var optionOne = new Option<bool>("--flag");
            var optionTwo = new Option<bool>("--flag2");

            var result = new RootCommand
                         {
                             optionOne,
                             optionTwo
                         }.Parse("@nonexistent.rsp");

            result.FindResultFor(optionOne).Should().BeNull();
            result.FindResultFor(optionTwo).Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be("Response file not found 'nonexistent.rsp'.");
        }

        [Fact]
        public void When_response_filepath_is_not_specified_then_error_is_returned()
        {
            var optionOne = new Option<bool>("--flag");
            var optionTwo = new Option<bool>("--flag2");

            var result = new RootCommand
                         {
                             optionOne,
                             optionTwo
                         }
                .Parse("@");

            result.FindResultFor(optionOne).Should().BeNull();
            result.FindResultFor(optionTwo).Should().BeNull();
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
            var optionOne = new Option<bool>("--flag");
            var optionTwo = new Option<bool>("--flag2");

            using (File.Open(nonexistent, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = new RootCommand
                             {
                                 optionOne,
                                 optionTwo
                             }.Parse($"@{nonexistent}");

                result.FindResultFor(optionOne).Should().BeNull();
                result.FindResultFor(optionTwo).Should().BeNull();
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

            var optionOne = new Option<string>("--flag");
            var optionTwo = new Option<int>("--flag2");

            var rootCommand = new RootCommand
            {
                optionOne,
                optionTwo
            };
            var config = new CommandLineBuilder(rootCommand)
                         .Build();

            var result = rootCommand.Parse($"@{responseFile}", config);

            result.GetValue(optionOne).Should().Be("first value");
            result.GetValue(optionTwo).Should().Be(123);
        }

        [Fact]
        public void When_response_file_processing_is_disabled_then_it_returns_response_file_name_as_argument()
        {
            var command = new RootCommand
            {
                new Argument<List<string>>("arg")
            };
            var configuration = new CommandLineConfiguration(
                command,
                enableTokenReplacement: false);
            
            var result = Parser.Parse(command, "@file.rsp", configuration);

            result.Tokens
                  .Should()
                  .Contain(t => t.Value == "@file.rsp" && 
                                t.Type == TokenType.Argument);
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void Response_files_can_refer_to_other_response_files()
        {
            var file3 = CreateResponseFile("--three", "3");
            var file2 = CreateResponseFile($"@{file3}", "--two", "2");
            var file1 = CreateResponseFile("--one", "1", $"@{file2}");

            var option1 = new Option<int>("--one");
            var option2 = new Option<int>("--two");
            var option3 = new Option<int>("--three");

            var command = new RootCommand
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

            var option1 = new Option<string>("--option1");
            var option2 = new Option<int>("--option2");

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_leading_spaces_they_are_ignored()
        {
            var responseFile = CreateResponseFile(" --option1", " value1", "\t--option2", "\t2");

            var option1 = new Option<string>("--option1");
            var option2 = new Option<int>("--option2");

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_trailing_and_leading_spaces_they_are_ignored()
        {
            var responseFile = CreateResponseFile(" --option1 ", " value1 ", "\t--option2\t", "\t2\t");

            var option1 = new Option<string>("--option1");
            var option2 = new Option<int>("--option2");

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.GetValue(option1).Should().Be("value1");
            result.GetValue(option2).Should().Be(2);
            result.Errors.Should().BeEmpty();
        }
    }
}
