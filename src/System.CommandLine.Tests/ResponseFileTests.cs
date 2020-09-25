// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
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
        private readonly List<FileInfo> _responseFiles = new List<FileInfo>();

        public void Dispose()
        {
            foreach (var responseFile in _responseFiles)
            {
                responseFile.Delete();
            }
        }

        private string ResponseFile(params string[] lines)
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
            var result = new Option("--flag")
                .Parse($"@{ResponseFile("--flag")}");

            result.HasOption("--flag").Should().BeTrue();
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_options_with_arguments_from_response_file()
        {
            var responseFile = ResponseFile(
                "--flag",
                "--flag2",
                "123");

            var result = new RootCommand
                         {
                             new Option("--flag"),
                             new Option("--flag2")
                             {
                                 Argument = new Argument<int>()
                             }
                         }
                .Parse($"@{responseFile}");

            result.HasOption("--flag").Should().BeTrue();
            result.ValueForOption("--flag2").Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_is_specified_it_loads_command_arguments_from_response_file()
        {
            var responseFile = ResponseFile(
                "one",
                "two",
                "three");

            var result = new RootCommand
                         {
                             new Argument<string[]>()
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
            var responseFile = ResponseFile(
                "one",
                "two",
                "three");

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>()
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
            var responseFile = ResponseFile("subcommand");

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>()
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
            var responseFile = ResponseFile(
                "one",
                "two",
                "three");

            var result = new RootCommand
                         {
                             new Command("subcommand")
                             {
                                 new Argument<string[]>()
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
            var responseFile = ResponseFile(
                "--flag",
                "",
                "123");

            var result = new CommandLineBuilder()
                         .AddOption(new Option("--flag")
                         {
                             Argument = new Argument<int>()
                         })
                         .Build()
                         .Parse($"@{responseFile}");

            result.ValueForOption("--flag").Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Response_file_can_contain_comments_which_are_ignored_when_loaded()
        {
            var responseFile = ResponseFile(
                "# comment one",
                "--flag",
                "# comment two",
                "#",
                " # comment two",
                "--flag2");

            var result = new RootCommand
                         {
                             new Option("--flag"),
                             new Option("--flag2")
                         }.Parse($"@{responseFile}");

            result.HasOption("--flag").Should().BeTrue();
            result.HasOption("--flag2").Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_does_not_exist_then_error_is_returned()
        {
            var result = new RootCommand
                         {
                             new Option("--flag"),
                             new Option("--flag2")
                         }.Parse("@nonexistent.rsp");

            result.HasOption("--flag").Should().BeFalse();
            result.HasOption("--flag2").Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be("Response file not found 'nonexistent.rsp'");
        }

        [Fact]
        public void When_response_filepath_is_not_specified_then_error_is_returned()
        {
            var result = new RootCommand
                         {
                             new Option("--flag"),
                             new Option("--flag2")
                         }
                .Parse("@");

            result.HasOption("--flag").Should().BeFalse();
            result.HasOption("--flag2").Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors
                  .Single()
                  .Message
                  .Should()
                  .Be("Unrecognized command or argument '@'");
        }

        [Fact]
        public void When_response_file_cannot_be_read_then_specified_error_is_returned()
        {
            var nonexistent = Path.GetTempFileName();

            using (File.Open(nonexistent, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = new RootCommand
                             {
                                 new Option("--flag"),
                                 new Option("--flag2")
                             }.Parse($"@{nonexistent}");

                result.HasOption("--flag").Should().BeFalse();
                result.HasOption("--flag2").Should().BeFalse();
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
            var responseFile = ResponseFile(input);

            var rootCommand = new RootCommand
            {
                new Option("--flag")
                {
                    Argument = new Argument<string>()
                },
                new Option("--flag2")
                {
                    Argument = new Argument<int>()
                }
            };
            var parser = new CommandLineBuilder(rootCommand)
                         .ParseResponseFileAs(ResponseFileHandling.ParseArgsAsSpaceSeparated)
                         .Build();

            var result = parser.Parse($"@{responseFile}");

            result.ValueForOption("--flag").Should().Be("first value");
            result.ValueForOption("--flag2").Should().Be(123);
        }

        [Fact]
        public void When_response_file_processing_is_disabled_then_it_returns_response_file_name_as_argument()
        {
            var command = new RootCommand
            {
                new Argument<List<string>>()
            };
            var configuration = new CommandLineConfiguration(
                new[] { command },
                responseFileHandling: ResponseFileHandling.Disabled);
            var parser = new Parser(configuration);

            var result = parser.Parse("@file.rsp");

            result.Tokens
                  .Should()
                  .Contain(t => t.Value == "@file.rsp" && 
                                t.Type == TokenType.Argument);
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void Response_files_can_refer_to_other_response_files()
        {
            var file3 = ResponseFile("--three", "3");
            var file2 = ResponseFile($"@{file3}", "--two", "2");
            var file1 = ResponseFile("--one", "1", $"@{file2}");

            var option1 = new Option("--one") { Argument = new Argument<int>() };
            var option2 = new Option("--two") { Argument = new Argument<int>() };
            var option3 = new Option("--three") { Argument = new Argument<int>() };

            var command = new RootCommand
                          {
                              option1,
                              option2,
                              option3
                          };

            var result = command.Parse($"@{file1}");

            result.FindResultFor(option1).GetValueOrDefault().Should().Be(1);
            result.FindResultFor(option1).GetValueOrDefault().Should().Be(1);
            result.FindResultFor(option2).GetValueOrDefault().Should().Be(2);
            result.FindResultFor(option3).GetValueOrDefault().Should().Be(3);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_trailing_spaces_they_are_ignored()
        {
            var responseFile = ResponseFile("--option1 ", "value1 ", "--option2\t", "2\t");

            var option1 = new Option("--option1") { Argument = new Argument<string>() };
            var option2 = new Option("--option2") { Argument = new Argument<int>() };

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.ValueForOption("--option1").Should().Be("value1");
            result.ValueForOption("--option2").Should().Be(2);
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_leading_spaces_they_are_ignored()
        {
            var responseFile = ResponseFile(" --option1", " value1", "\t--option2", "\t2");

            var option1 = new Option("--option1") { Argument = new Argument<string>() };
            var option2 = new Option("--option2") { Argument = new Argument<int>() };

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.ValueForOption("--option1").Should().Be("value1");
            result.ValueForOption("--option2").Should().Be(2);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_options_or_arguments_contain_trailing_and_leading_spaces_they_are_ignored()
        {
            var responseFile = ResponseFile(" --option1 ", " value1 ", "\t--option2\t", "\t2\t");

            var option1 = new Option("--option1") { Argument = new Argument<string>() };
            var option2 = new Option("--option2") { Argument = new Argument<int>() };

            var result = new RootCommand { option1, option2 }.Parse($"@{responseFile}");
            result.ValueForOption("--option1").Should().Be("value1");
            result.ValueForOption("--option2").Should().Be(2);
            result.Errors.Should().BeEmpty();
        }
    }
}
