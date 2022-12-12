﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
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
            var option = new Option<string>("-x")
                .AcceptOnlyFromAmong("this", "that", "the-other-thing");

            var result = option.Parse("-x none-of-those");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain("Argument 'none-of-those' not recognized. Must be one of:\n\t'this'\n\t'that'\n\t'the-other-thing'");
        }

        [Fact]
        public void When_an_option_has_en_error_then_the_error_has_a_reference_to_the_option()
        {
            var option = new Option<string>("-x")
                .AcceptOnlyFromAmong("this", "that");

            var result = option.Parse("-x something_else");

            result.Errors
                  .Where(e => e.SymbolResult != null)
                  .Should()
                  .Contain(e => e.SymbolResult.Symbol.Name == option.Name);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1475
        public void When_FromAmong_is_used_then_the_OptionResult_ErrorMessage_is_set()
        {
            var option = new Option<string>("--opt").AcceptOnlyFromAmong("a", "b");
            var command = new Command("test") { option };

            var parseResult = command.Parse("test --opt c");

            parseResult.FindResultFor(option)
                       .ErrorMessage
                       .Should()
                       .Be(parseResult.Errors.Single().Message)
                       .And
                       .Should()
                       .NotBeNull();
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1475
        public void When_FromAmong_is_used_then_the_ArgumentResult_ErrorMessage_is_set()
        {
            var option = new Argument<string>().AcceptOnlyFromAmong("a", "b");
            var command = new Command("test") { option };

            var parseResult = command.Parse("test c");

            parseResult.FindResultFor(option)
                       .ErrorMessage
                       .Should()
                       .Be(parseResult.Errors.Single().Message)
                       .And
                       .Should()
                       .NotBeNull();
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1556
        public void When_FromAmong_is_used_for_multiple_arguments_and_valid_input_is_provided_then_there_are_no_errors()
        {
            var command = new Command("set")
            {
                new Argument<string>("key").AcceptOnlyFromAmong("key1", "key2"),
                new Argument<string>("value").AcceptOnlyFromAmong("value1", "value2")
            };

            var result = command.Parse("set key1 value1");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_FromAmong_is_used_for_multiple_arguments_and_invalid_input_is_provided_for_the_first_one_then_the_error_is_informative()
        {
            var command = new Command("set")
            {
                new Argument<string>("key").AcceptOnlyFromAmong("key1", "key2"),
                new Argument<string>("value").AcceptOnlyFromAmong("value1", "value2")
            };

            var result = command.Parse("set not-key1 value1");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which
                  .Message
                  .Should()
                  .Be(LocalizationResources.Instance.UnrecognizedArgument("not-key1", new[] { "key1", "key2" }));
        }

        [Fact]
        public void When_FromAmong_is_used_for_multiple_arguments_and_invalid_input_is_provided_for_the_second_one_then_the_error_is_informative()
        {
            var command = new Command("set")
            {
                new Argument<string>("key").AcceptOnlyFromAmong("key1", "key2"),
                new Argument<string>("value").AcceptOnlyFromAmong("value1", "value2")
            };

            var result = command.Parse("set key1 not-value1");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which
                  .Message
                  .Should()
                  .Be(LocalizationResources.Instance.UnrecognizedArgument("not-value1", new[] { "value1", "value2" }));
        }

        [Fact]
        public void When_a_required_argument_is_not_supplied_then_an_error_is_returned()
        {
            var option = new Option<string>("-x");

            var result = option.Parse("-x");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.Message == "Required argument missing for option: '-x'.");
        }

        [Fact]
        public void When_a_required_option_is_not_supplied_then_an_error_is_returned()
        {
            var command = new Command("command")
            {
                new Option<string>("-x")
                {
                    IsRequired = true
                }
            };

            var result = command.Parse("");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.SymbolResult.Symbol == command)
                  .Which
                  .Message
                  .Should()
                  .Be("Option '-x' is required.");
        }

        [Fact]
        public void When_a_required_option_has_multiple_aliases_the_error_message_uses_longest()
        {
            var command = new Command("command")
            {
                new Option<string>(new[] {"-x", "--xray" })
                {
                    IsRequired = true
                }
            };

            var result = command.Parse("");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.SymbolResult.Symbol == command)
                  .Which
                  .Message
                  .Should()
                  .Be("Option '--xray' is required.");
        }

        [Theory]
        [InlineData("subcommand -x arg")]
        [InlineData("-x arg subcommand")]
        public void When_a_required_option_is_allowed_at_more_than_one_position_it_only_needs_to_be_satisfied_in_one(string commandLine)
        {
            var option = new Option<string>("-x")
            {
                IsRequired = true
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
                new Option<string>("-x") { IsRequired = true },
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
                    new Option<bool>("-x")
                    {
                        Arity = ArgumentArity.Zero
                    }
                });

            var result = parser.Parse("the-command -x some-arg");

            _output.WriteLine(result.ToString());

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e == "Unrecognized command or argument 'some-arg'.");
        }

        [Fact]
        public void A_custom_validator_can_be_added_to_a_command()
        {
            var command = new Command("the-command")
            {
                new Option<bool>("--one"),
                new Option<bool>("--two")
            };

            command.AddValidator(commandResult =>
            {
                if (commandResult.Children.Any(sr => sr.Symbol is IdentifierSymbol id && id.HasAlias("--one")) &&
                    commandResult.Children.Any(sr => sr.Symbol is IdentifierSymbol id && id.HasAlias("--two")))
                {
                    commandResult.ErrorMessage = "Options '--one' and '--two' cannot be used together.";
                }
            });

            var result = command.Parse("the-command --one --two");

            result
                .Errors
                .Select(e => e.Message)
                .Should()
                .HaveCount(1)
                .And
                .Contain("Options '--one' and '--two' cannot be used together.");
        }

        [Fact]
        public void A_custom_validator_can_be_added_to_an_option()
        {
            var option = new Option<int>("-x");

            option.AddValidator(r =>
            {
                var value = r.GetValueOrDefault<int>();

                r.ErrorMessage = $"Option {r.Token.Value} cannot be set to {value}";
            });

            var command = new RootCommand { option };

            var result = command.Parse("-x 123");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.SymbolResult.Symbol == option)
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

                r.ErrorMessage = $"Argument {r.Argument.Name} cannot be set to {value}";
            });

            var command = new RootCommand { argument };

            var result = command.Parse("123");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.SymbolResult.Symbol == argument)
                  .Which
                  .Message
                  .Should()
                  .Be("Argument x cannot be set to 123");
        }

        [Theory]
        [InlineData("-o=optionValue argValue")]
        [InlineData("argValue -o=optionValue")]
        public void All_custom_validators_are_called(string commandLine)
        {
            var commandValidatorWasCalled = false;
            var optionValidatorWasCalled = false;
            var argumentValidatorWasCalled = false;

            var option = new Option<string>("-o");
            option.AddValidator(_ =>
            {
                optionValidatorWasCalled = true;
            });

            var argument = new Argument<string>("the-arg");
            argument.AddValidator(_ =>
            {
                argumentValidatorWasCalled = true;
            });

            var rootCommand = new RootCommand
            {
                option,
                argument
            };
            rootCommand.AddValidator(_ =>
            {
                commandValidatorWasCalled = true;
            });

            rootCommand.Invoke(commandLine);

            commandValidatorWasCalled.Should().BeTrue();
            optionValidatorWasCalled.Should().BeTrue();
            argumentValidatorWasCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("--file \"Foo\" subcommand")]
        [InlineData("subcommand --file \"Foo\"")]
        public void Validators_on_global_options_are_executed_when_invoking_a_subcommand(string commandLine)
        {
            var option = new Option<FileInfo>("--file");
            option.AddValidator(r =>
            {
                r.ErrorMessage = "Invoked validator";
            });

            var subCommand = new Command("subcommand");
            var rootCommand = new RootCommand 
            {
                subCommand
            };
            rootCommand.AddGlobalOption(option);

            var result = rootCommand.Parse(commandLine);

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.SymbolResult.Symbol == option)
                  .Which
                  .Message
                  .Should()
                  .Be("Invoked validator");
        }

        [Theory]
        [InlineData("--value 123")]
        [InlineData("--value=123 child")]
        [InlineData("--value=123 child grandchild")]
        [InlineData("child --value=123")]
        [InlineData("child --value=123 grandchild")]
        [InlineData("child grandchild --value=123 ")]
        public async Task A_custom_validator_added_to_a_global_option_is_checked(string commandLine)
        {
            var handlerWasCalled = false;

            var globalOption = new Option<int>("--value");
            globalOption.AddValidator(r => r.ErrorMessage = "oops!");

            var grandchildCommand = new Command("grandchild");

            var childCommand = new Command("child")
            {
                grandchildCommand
            };
            var rootCommand = new RootCommand
            {
                childCommand
            };

            rootCommand.AddGlobalOption(globalOption);

            rootCommand.SetHandler((int i) => handlerWasCalled = true, globalOption);
            childCommand.SetHandler((int i) => handlerWasCalled = true, globalOption);
            grandchildCommand.SetHandler((int i) => handlerWasCalled = true, globalOption);

            var result = await rootCommand.InvokeAsync(commandLine);

            result.Should().Be(1);
            handlerWasCalled.Should().BeFalse();
        }

        [Fact]
        public void Custom_validator_error_messages_are_not_repeated()
        {
            var errorMessage = "that's not right...";
            var argument = new Argument<string>();
            argument.AddValidator(r => r.ErrorMessage = errorMessage);

            var cmd = new Command("get")
            {
                argument
            };

            var result = cmd.Parse("get something");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.Message == errorMessage);
        }

        [Fact]
        public void The_parsed_value_of_an_argument_is_available_within_a_validator()
        {
            var argument = new Argument<int>();
            var errorMessage = "The value of option '-x' must be between 1 and 100.";
            argument.AddValidator(result =>
            {
                var value = result.GetValue(argument);

                if (value < 0 || value > 100)
                {
                    result.ErrorMessage = errorMessage;
                }
            });

            var result = argument.Parse("-1");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.Message == errorMessage);
        }

        [Fact]
        public void The_parsed_value_of_an_option_is_available_within_a_validator()
        {
            var option = new Option<int>("-x");
            var errorMessage = "The value of option '-x' must be between 1 and 100.";
            option.AddValidator(result =>
            {
                var value = result.GetValue(option);

                if (value < 0 || value > 100)
                {
                    result.ErrorMessage = errorMessage;
                }
            });

            var result = option.Parse("-x -1");

            result.Errors
                  .Should()
                  .HaveCount(1)
                  .And
                  .Contain(e => e.Message == errorMessage);
        }

        public class PathValidity
        {
            [Fact]
            public void LegalFilePathsOnly_rejects_command_arguments_containing_invalid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>().AcceptLegalFilePathsOnly()
                };

                var invalidCharacter = Path.GetInvalidPathChars().First(c => c != '"');

                var result = command.Parse($"the-command {invalidCharacter}");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol == command.Arguments.First() &&
                                    e.Message == $"Character not allowed in a path: '{invalidCharacter}'.");
            }   
            
            [Fact]
            public void LegalFilePathsOnly_rejects_option_arguments_containing_invalid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Option<string>("-x").AcceptLegalFilePathsOnly()
                };

                var invalidCharacter = Path.GetInvalidPathChars().First(c => c != '"');

                var result = command.Parse($"the-command -x {invalidCharacter}");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "x" &&
                                    e.Message == $"Character not allowed in a path: '{invalidCharacter}'.");
            }

            [Fact]
            public void LegalFilePathsOnly_accepts_command_arguments_containing_valid_path_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string[]>().AcceptLegalFilePathsOnly()
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
                    new Option<string[]>("-x").AcceptLegalFilePathsOnly()
                };

                var validPathName = Directory.GetCurrentDirectory();
                var validNonExistingFileName = Path.Combine(validPathName, Guid.NewGuid().ToString());

                var result = command.Parse($"the-command -x {validPathName} -x {validNonExistingFileName}");

                result.Errors.Should().BeEmpty();
            }
        }

        public class FileNameValidity
        {
            [Fact]
            public void LegalFileNamesOnly_rejects_command_arguments_containing_invalid_file_name_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>().AcceptLegalFileNamesOnly()
                };

                var invalidCharacter = Path.GetInvalidFileNameChars().First(c => c != '"');

                var result = command.Parse($"the-command {invalidCharacter}");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol == command.Arguments.First() &&
                                    e.Message == $"Character not allowed in a file name: '{invalidCharacter}'.");
            }

            [Fact]
            public void LegalFileNamesOnly_rejects_option_arguments_containing_invalid_file_name_characters()
            {
                var command = new Command("the-command")
                {
                    new Option<string>("-x").AcceptLegalFileNamesOnly()
                };

                var invalidCharacter = Path.GetInvalidFileNameChars().First(c => c != '"');

                var result = command.Parse($"the-command -x {invalidCharacter}");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "x" &&
                                    e.Message == $"Character not allowed in a file name: '{invalidCharacter}'.");
            }

            [Fact]
            public void LegalFileNamesOnly_accepts_command_arguments_containing_valid_file_name_characters()
            {
                var command = new Command("the-command")
                {
                    new Argument<string[]>().AcceptLegalFileNamesOnly()
                };

                var validFileName = Path.GetFileName(Directory.GetCurrentDirectory());
                var validNonExistingFileName = Guid.NewGuid().ToString();

                var result = command.Parse($"the-command {validFileName} {validNonExistingFileName}");

                result.Errors.Should().BeEmpty();
            }

            [Fact]
            public void LegalFileNamesOnly_accepts_option_arguments_containing_valid_file_name_characters()
            {
                var command = new Command("the-command")
                {
                    new Option<string[]>("-x").AcceptLegalFileNamesOnly()
                };

                var validFileName = Path.GetFileName(Directory.GetCurrentDirectory());
                var validNonExistingFileName = Guid.NewGuid().ToString();

                var result = command.Parse($"the-command -x {validFileName} -x {validNonExistingFileName}");

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
                    new Argument<FileInfo>("to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileInfo>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<DirectoryInfo>("to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<DirectoryInfo>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"Directory does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_can_be_invalid_based_on_file_or_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo>().AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($"move \"{path}\"");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol == command.Arguments.First() &&
                                    e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_can_be_invalid_based_on_file_or_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" &&
                                    e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_with_multiple_files_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Argument<IEnumerable<FileInfo>>("to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" && 
                                    e.Message == $"File does not exist: '{path}'.");
            }
            
            [Fact]
            public void An_option_argument_with_multiple_files_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<IEnumerable<FileInfo>>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .Contain(e => e.SymbolResult.Symbol.Name == "to" && 
                                    e.Message == $"File does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_with_multiple_directories_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<List<DirectoryInfo>>("to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"Directory does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_with_multiple_directories_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<DirectoryInfo[]>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"Directory does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo[]>("to")
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }.AcceptExistingOnly(),
                    new Option<string>("--to")
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_file_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo[]>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result =
                    command.Parse(
                        $@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void A_command_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Argument<FileSystemInfo>("to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void An_option_argument_with_multiple_FileSystemInfos_can_be_invalid_based_on_directory_existence()
            {
                var command = new Command("move")
                {
                    new Option<FileSystemInfo[]>("--to").AcceptExistingOnly()
                };

                var path = NonexistentPath();
                var result = command.Parse($@"move --to ""{path}""");

                result.Errors
                      .Should()
                      .HaveCount(1)
                      .And
                      .ContainSingle(e => e.SymbolResult.Symbol.Name == "to" &&
                                          e.Message == $"File or directory does not exist: '{path}'.");
            }

            [Fact]
            public void Command_argument_does_not_return_errors_when_file_exists()
            {
                var command = new Command("move")
                {
                    new Argument<FileInfo>().AcceptExistingOnly()
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
                    new Option<FileInfo>("--to").AcceptExistingOnly()
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
                    new Argument<DirectoryInfo>().AcceptExistingOnly()
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
                    new Option<DirectoryInfo>("--to").AcceptExistingOnly()
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
                return new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles()[0].FullName;
            }
        }

        [Fact]
        public void A_command_with_subcommands_is_invalid_to_invoke_if_it_has_no_handler()
        {
            var outer = new Command("outer");
            var inner = new Command("inner");
            var innerer = new Command("inner-er");
            outer.Subcommands.Add(inner);
            inner.Subcommands.Add(innerer);

            var result = outer.Parse("outer inner arg");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message.Equals(LocalizationResources.Instance.RequiredCommandWasNotProvided()) &&
                           e.SymbolResult.Symbol.Name.Equals("inner"));
        }

        [Fact]
        public void A_root_command_is_invalid_if_it_has_no_handler()
        {
            var rootCommand = new RootCommand();
            var inner = new Command("inner");
            rootCommand.Add(inner);

            var result = rootCommand.Parse("");

            result.Errors
                  .Should()
                  .ContainSingle(
                      e => e.Message.Equals(LocalizationResources.Instance.RequiredCommandWasNotProvided()) &&
                           e.SymbolResult.Symbol == rootCommand);
        }

        [Fact]
        public void A_command_with_subcommands_is_valid_to_invoke_if_it_has_a_handler()
        {
            var outer = new Command("outer");
            var inner = new Command("inner");
            inner.SetHandler(() => { });
            var innerer = new Command("inner-er");
            outer.Subcommands.Add(inner);
            inner.Subcommands.Add(innerer);

            var result = outer.Parse("outer inner");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Should().BeSameAs(inner);
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_is_not_valid_to_specify_the_option_without_an_argument()
        {
            var option = new Option<int>("-x", () => 123);

            var result = option.Parse("-x");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Required argument missing for option: '-x'.");
        }

        [Fact]
        public void When_an_option_has_a_default_value_then_the_default_should_apply_if_not_specified()
        {
            var optionX = new Option<int>("-x", () => 123);
            var optionY = new Option<int>("-y", () => 456);

            var parser = new RootCommand
            {
                optionX,
                optionY
            };

            var result = parser.Parse("");

            result.Errors.Should().BeEmpty();
            result.GetValue(optionX).Should().Be(123);
            result.GetValue(optionY).Should().Be(456);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1505
        public void Arity_failures_are_not_reported_for_both_an_argument_and_its_parent_option()
        {
            var newCommand = new Command("test")
            {
                new Option<string>("--opt")
            };

            var parseResult = newCommand.Parse("test --opt");
            
            parseResult.Errors
                       .Should()
                       .ContainSingle()
                       .Which
                       .Message
                       .Should()
                       .Be("Required argument missing for option: '--opt'.");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1573
        public void Multiple_validators_on_the_same_command_do_not_report_duplicate_errors()
        {
            var command = new RootCommand();
            command.AddValidator(result => result.ErrorMessage = "Wrong");
            command.AddValidator(_ => { });

            var parseResult = command.Parse("");

            parseResult.Errors
                       .Should()
                       .ContainSingle()
                       .Which
                       .Message
                       .Should()
                       .Be("Wrong");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1573
        public void Multiple_validators_on_the_same_option_do_not_report_duplicate_errors()
        {
            var option = new Option<string>("-x");
            option.AddValidator(result => result.ErrorMessage = "Wrong");
            option.AddValidator(_ => { });

            var command = new RootCommand
            {
                option
            };

            var parseResult = command.Parse("-x b");

            parseResult.Errors
                       .Should()
                       .ContainSingle()
                       .Which
                       .Message
                       .Should()
                       .Be("Wrong");
        }   
        
        [Fact] // https://github.com/dotnet/command-line-api/issues/1573
        public void Multiple_validators_on_the_same_argument_do_not_report_duplicate_errors()
        {
            var argument = new Argument<string>();
            argument.AddValidator(result => result.ErrorMessage = "Wrong");
            argument.AddValidator(_ => { });

            var command = new RootCommand
            {
                argument
            };

            var parseResult = command.Parse("b");

            parseResult.Errors
                       .Should()
                       .ContainSingle()
                       .Which
                       .Message
                       .Should()
                       .Be("Wrong");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1609
        internal void When_there_is_an_arity_error_then_further_errors_are_not_reported()
        {
            var option = new Option<string>("-o");
            option.AddValidator(result =>
            {
                result.ErrorMessage = "OOPS";
            }); //all good;

            var command = new Command("comm")
            {
                option
            };

            var parseResult = command.Parse("comm -o");

            parseResult.Errors
                       .Should()
                       .ContainSingle()
                       .Which
                       .Message
                       .Should()
                       .Be("Required argument missing for option: '-o'.");
        }
    }
}
