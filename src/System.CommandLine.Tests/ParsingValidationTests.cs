﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParsingValidationTests
    {
        private readonly ITestOutputHelper _output;

        public ParsingValidationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void When_an_option_accepts_only_specific_arguments_but_a_wrong_one_is_supplied_then_an_informative_error_is_returned()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = new Argument
                        {
                            Arity = ArgumentArity.ExactlyOne 
                        }
                        .FromAmong("this", "that", "the-other-thing")
                });

            var result = parser.Parse("-x none-of-those");

            result.Errors
                  .Select(e => e.Message)
                  .Single()
                  .Should()
                  .Contain("Argument 'none-of-those' not recognized. Must be one of:\n\t'this'\n\t'that'\n\t'the-other-thing'");
        }

        [Fact]
        public void When_an_option_has_en_error_then_the_error_has_a_reference_to_the_option()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }.FromAmong("this", "that")
            };

            var parser = new Parser(option);

            var result = parser.Parse("-x something_else");

            result.Errors
                  .Where(e => e.SymbolResult != null)
                  .Should()
                  .Contain(e => e.SymbolResult.Symbol.Name == option.Name);
        }

        [Fact]
        public void When_a_required_argument_is_not_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            });

            var result = parser.Parse("-x");

            result.Errors
                  .Should()
                  .Contain(e => e.Message == "Required argument missing for option: -x");
        }

        
        [Fact]
        public void When_a_required_option_is_not_supplied_then_an_error_is_returned()
        {
            var command = new Command("command")
            {
                new Option("-x")
                {
                    Required = true
                }
            };

            var result = command.Parse("");

            result.Errors
                  .Should()
                  .ContainSingle(e => e.SymbolResult.Symbol == command)
                  .Which
                  .Message
                  .Should()
                  .Be("Option '-x' is required.");
        }

        [Theory]
        [InlineData("subcommand -x arg")]
        [InlineData("-x arg subcommand")]
        public void When_a_required_option_is_allowed_at_more_than_one_position_it_only_needs_to_be_satisfied_in_one(string commandLine)
        {
            var option = new Option<string>("-x")
            {
                Required = true
            };

            var command = new RootCommand
            {
                option,
                new Command("subcommand")
                {
                    option
                }
            };

            var result = command.Parse(commandLine);

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Required_options_on_parent_commands_do_not_create_parse_errors_when_an_inner_command_is_specified()
        {
            var child = new Command("child");

            var parent = new RootCommand
            {
                new Option<string>("-x") { Required = true },
                child
            };
            parent.Name = "parent";

            var result = parent.Parse("child");

            result.Errors.Should().BeEmpty();
        }


        [Fact]
        public void When_no_option_accepts_arguments_but_one_is_supplied_then_an_error_is_returned()
        {
            var parser = new Parser(
                new Command("the-command")
                {
                    new Option("-x")
                });

            var result = parser.Parse("the-command -x some-arg");

            _output.WriteLine(result.ToString());

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .ContainSingle(e => e == "Unrecognized command or argument 'some-arg'");
        }

        [Fact]
        public void A_custom_validator_can_be_added_to_a_command()
        {
            var command = new Command("the-command")
            {
                new Option("--one"),
                new Option("--two")
            };

            command.AddValidator(commandResult =>
            {
                if (commandResult.Children.Contains("one") &&
                    commandResult.Children.Contains("two"))
                {
                    return "Options '--one' and '--two' cannot be used together.";
                }

                return null;
            });

            var result = command.Parse("the-command --one --two");

            result
                .Errors
                .Select(e => e.Message)
                .Should()
                .ContainSingle("Options '--one' and '--two' cannot be used together.");
        }

        [Fact]
        public void A_custom_validator_can_be_added_to_an_option()
        {
            var option = new Option<int>("-x");

            option.AddValidator(r =>
            {
                var value = r.GetValueOrDefault<int>();

                return $"Option {r.Token.Value} cannot be set to {value}";
            });

            var command = new RootCommand { option };

            var result = command.Parse("-x 123");

            result.Errors
                  .Should()
                  .ContainSingle(e => e.SymbolResult.Symbol == option)
                  .Which
                  .Message
                  .Should()
                  .Be("Option -x cannot be set to 123");
        }

        [Fact]
        public void A_custom_validator_can_be_added_to_an_argument()
        {
            var argument = new Argument<int>("x");

            argument.AddValidator(r =>
            {
                var value = r.GetValueOrDefault<int>();

                return $"Argument {r.Argument.Name} cannot be set to {value}";
            });

            var command = new RootCommand { argument };

            var result = command.Parse("123");

            result.Errors
                  .Should()
                  .ContainSingle(e => e.SymbolResult.Symbol == argument)
                  .Which
                  .Message
                  .Should()
                  .Be("Argument x cannot be set to 123");
        }

        [Fact]
        public void Custom_validator_error_messages_are_not_repeated()
        {
            var errorMessage = "that's not right...";
            var argument = new Argument<string>();
            argument.AddValidator(o => errorMessage);

            var cmd = new Command("get")
            {
                argument
            };

            var result =  cmd.Parse("get something");

            result.Errors
                  .Should()
                  .ContainSingle(errorMessage);
        }

        public class PathValidity
        {
            [Fact]
            public void LegalFilePathsOnly_rejects_command_arguments_containing_invalid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>().LegalFilePathsOnly()
                };

                var invalidCharacter = Path.GetInvalidPathChars().First(c => c != '"');

                var result = command.Parse($"the-command {invalidCharacter}");

                result.Errors
                      .Should()
                      .Contain(e => e.SymbolResult.Symbol.Name == "the-command" &&
                                    e.Message == $"Character not allowed in a path: {invalidCharacter}");
            }   
            
            [Fact]
            public void LegalFilePathsOnly_rejects_option_arguments_containing_invalid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Option<string>("-x").LegalFilePathsOnly()
                };

                var invalidCharacter = Path.GetInvalidPathChars().First(c => c != '"');

                var result = command.Parse($"the-command -x {invalidCharacter}");

                result.Errors
                      .Should()
                      .Contain(e => e.SymbolResult.Symbol.Name == "x" &&
                                    e.Message == $"Character not allowed in a path: {invalidCharacter}");
            }

            [Fact]
            public void LegalFilePathsOnly_accepts_command_arguments_containing_valid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string[]>().LegalFilePathsOnly()
                };

                var validPathName = Directory.GetCurrentDirectory();
                var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

                var result = command.Parse($"the-command {validPathName} {validNonExistingFileName}");

                result.Errors.Should().BeEmpty();
            }
            
            [Fact]
            public void LegalFilePathsOnly_accepts_option_arguments_containing_valid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Option<string[]>("-x").LegalFilePathsOnly()
                };

                var validPathName = Directory.GetCurrentDirectory();
                var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

                var result = command.Parse($"the-command -x {validPathName} {validNonExistingFileName}");

                result.Errors.Should().BeEmpty();
            }
        }

        public class FileExistence
        {
            [Fact]
            public void A_command_argument_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileInfo>("to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileInfo>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<DirectoryInfo>("to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<DirectoryInfo>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_can_be_invalid_based_on_file_or_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo>().ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($"move \"{path}\"");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "move" &&
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_file_or_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_with_multiple_files_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Argument<IEnumerable<FileInfo>>("to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" && 
                                    e.Message == $"File does not exist: {path}");
            }
            
            [Fact]
            public void An_option_argument_with_multiple_files_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<IEnumerable<FileInfo>>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" && 
                                    e.Message == $"File does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_with_multiple_directories_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<List<DirectoryInfo>>("to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_with_multiple_directories_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<DirectoryInfo[]>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo[]>("to")
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }.ExistingOnly(),
                    new Option("--to")
                    {
                        Argument = new Argument
                        {
                            Arity = ArgumentArity.ExactlyOne
                        }
                    }
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" && 
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo[]>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result =
                    command.Parse(
                        $@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void A_command_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo>("to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void An_option_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo[]>("--to").ExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File or directory does not exist: {path}");
            }

            [Fact]
            public void Command_argument_does_not_return_errors_when_file_exists()
            {
                var command = new Command("move")
                {
                    new Argument<FileInfo>().ExistingOnly()
                };

                var path = ExistingFile();
                var result = command.Parse($@"move ""{path}""");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Option_argument_does_not_return_errors_when_file_exists()
            {
                var command = new Command("move")
                {
                    new Option<FileInfo>("--to").ExistingOnly()
                };

                var path = ExistingFile();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Command_argument_does_not_return_errors_when_Directory_exists()
            {
                var command = new Command("move")
                {
                    new Argument<DirectoryInfo>().ExistingOnly()
                };

                var path = ExistingDirectory();
                var result = command.Parse($@"move ""{path}""");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void Option_argument_does_not_return_errors_when_Directory_exists()
            {
                var command = new Command("move")
                {
                    new Option<DirectoryInfo>("--to").ExistingOnly()
                };

                var path = ExistingDirectory();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors.Should().BeEmpty();
            }

            private string NonexistentPath()
            {
                return Guid.NewGuid().ToString();
            }

            private string ExistingDirectory()
            {
                return Directory.GetCurrentDirectory();
            }
            
            private string ExistingFile()
            {
                return new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles().First().FullName;
            }
        }

        [Fact]
        public void A_command_with_subcommands_is_invalid_to_invoke_if_it_has_no_handler()
        {
            var outer = new Command("outer");
            var inner = new Command("inner");
            var innerer = new Command("inner-er");
            outer.AddCommand(inner);
            inner.AddCommand(innerer);

            var result = outer.Parse("outer inner arg");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message.Equals(ValidationMessages.Instance.RequiredCommandWasNotProvided()) &&
                           e.SymbolResult.Symbol.Name.Equals("inner"));
        }

        [Fact]
        public void A_command_with_subcommands_is_valid_to_invoke_if_it_has_a_handler()
        {
            var outer = new Command("outer");
            var inner = new Command("inner")
                        {
                            Handler = CommandHandler.Create(() =>
                            {
                            })
                        };
            var innerer = new Command("inner-er");
            outer.AddCommand(inner);
            inner.AddCommand(innerer);

            var result = outer.Parse("outer inner");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Should().Be(inner);
        }

        [Fact]
        public void When_an_option_is_specified_more_than_once_but_only_allowed_once_then_an_informative_error_is_returned()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Option '-x' expects a single argument but 2 were provided.");
        }

        [Fact]
        public void When_arity_is_ExactlyOne_it_validates_against_extra_arguments()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = new Argument<int>()
                });

            var result = parser.Parse("-x 1 -x 2");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Option '-x' expects a single argument but 2 were provided.");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_is_not_valid_to_specify_the_option_without_an_argument()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = new Argument<int>(() => 123)
                });

            var result = parser.Parse("-x");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Required argument missing for option: -x");
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_the_default_should_apply_if_not_specified()
        {
            var parser = new Parser(
                new Option("-x")
                {
                    Argument = (Argument) new Argument<int>(() => 123)
                },
                new Option("-y")
                {
                    Argument = (Argument) new Argument<int>(() => 456)
                });

            var result = parser.Parse("");

            result.Errors.Should().BeEmpty();
            result.RootCommandResult.ValueForOption("-x").Should().Be(123);
            result.RootCommandResult.ValueForOption("-y").Should().Be(456);
        }
    }
}
