// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public class HelpBuilderTests
    {
        private const int SmallMaxWidth = 70;
        private const int LargeMaxWidth = 200;
        private const int WindowMargin = 2;
        private const int ColumnGutterWidth = 4;
        private const int IndentationWidth = 2;

        private readonly HelpBuilder _helpBuilder;
        private readonly IConsole _console;
        private readonly ITestOutputHelper _output;
        private readonly string _executableName;
        private readonly string _columnPadding;
        private readonly string _indentation;

        public HelpBuilderTests(ITestOutputHelper output)
        {
            _console = new TestConsole();
            _helpBuilder = GetHelpBuilder(LargeMaxWidth);
            _output = output;
            _columnPadding = new string(' ', ColumnGutterWidth);
            _indentation = new string(' ', IndentationWidth);
            _executableName = RootCommand.ExeName;
        }

        private HelpBuilder GetHelpBuilder(int maxWidth)
        {
            return new HelpBuilder(
                console: _console,
                columnGutter: ColumnGutterWidth,
                indentationSize: IndentationWidth,
                maxWidth: maxWidth
            );
        }

        #region Synopsis

        [Fact]
        public void Synopsis_section_removes_extra_whitespace()
        {
            var command = new RootCommand(
                description: "test  description\tfor synopsis");

            _helpBuilder.Write(command);

            _output.WriteLine(_console.Out.ToString());

            var expected =
                $"{_executableName}:{NewLine}" +
                $"{_indentation}test description for synopsis{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Synopsis_section_removes_added_newlines()
        {
            var command = new RootCommand(
                $"test{NewLine}description with{NewLine}line breaks");

            _helpBuilder.Write(command);

            var expected =
                $"{_executableName}:{NewLine}" +
                $"{_indentation}test description with line breaks{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Synopsis_section_properly_wraps_description()
        {
            var longSynopsisText =
                $"test{NewLine}" +
                $"description with line breaks that is long enough to wrap to a{NewLine}" +
                $"new line";

            var command = new RootCommand(description: longSynopsisText);

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $@"{_executableName}:{NewLine}" +
                $"{_indentation}test description with line breaks that is long enough to wrap to a{NewLine}" +
                $"{_indentation}new line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Command_name_in_synopsis_can_be_specified()
        {
            var command = new RootCommand
                          {
                              Name = "custom-name"
                          };

            var helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected = $"custom-name{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
            _console.Out.ToString().Should().NotContain(_executableName);
        }

        #endregion Synopsis

        #region Usage

        [Theory]
        [InlineData(1, 1, "<the-args>")]
        [InlineData(1, 2, "<the-args>...")]
        [InlineData(0, 2, "[<the-args>...]")]
        public void Usage_section_shows_arguments_if_there_are_arguments_for_command_when_there_is_one_argument(
            int minArity,
            int maxArity,
            string expectedDescriptor)
        {
            var argument = new Argument
                           {
                               Name = "the-args",
                               Arity = new ArgumentArity(minArity, maxArity)
                           };
            var command = new Command("the-command", "command help")
            {
                argument,
                new Option(new[]
                {
                    "-v",
                    "--verbosity"
                })
                {
                    Description = "Sets the verbosity"
                }
            };
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(command);

            _helpBuilder.Write(command);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} the-command [options] {expectedDescriptor}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Theory]
        [InlineData(1, 1, 1, 1, "<arg1> <arg2>")]
        [InlineData(0, 1, 0, 1, "[<arg1> [<arg2>]]")]
        [InlineData(0, 1, 0, 2, "[<arg1> [<arg2>...]]")]
        public void Usage_section_shows_arguments_if_there_are_arguments_for_command_when_there_is_more_than_one_argument(
            int minArityForArg1,
            int maxArityForArg1,
            int minArityForArg2,
            int maxArityForArg2,
            string expectedDescriptor)
        {
            var arg1 = new Argument
                           {
                               Name = "arg1",
                               Arity = new ArgumentArity(
                                   minArityForArg1, 
                                   maxArityForArg1)
                           };
            var arg2 = new Argument
                           {
                               Name = "arg2",
                               Arity = new ArgumentArity(
                                   minArityForArg2, 
                                   maxArityForArg2)
                           };
            var command = new Command("the-command", "command help")
            {
                arg1,
                arg2,
                new Option(new[] { "-v", "--verbosity" })
                {
                    Description = "Sets the verbosity"
                }
            };
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(command);

            _helpBuilder.Write(command);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} the-command [options] {expectedDescriptor}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_for_subcommand_shows_names_of_parent_commands()
        {
            var outer = new Command("outer", "the outer command");
            var inner = new Command("inner", "the inner command");
            outer.AddCommand(inner);
            var innerEr = new Command("inner-er", "the inner-er command");
            inner.AddCommand(innerEr);
            innerEr.AddOption(new Option("--some-option", "some option"));
            var rootCommand = new RootCommand();
            rootCommand.Add(outer);

            _helpBuilder.Write(innerEr);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} outer inner inner-er [options]";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_for_subcommand_shows_arguments_for_subcommand_and_parent_command()
        {
            var inner = new Command("inner", "command help")
            {
                new Option("-v", "Sets the verbosity"),
                new Argument<string[]>
                {
                    Name = "inner-args"
                }
            };

            var outer = new Command("outer", "command help")
            {
                inner,
                new Argument<string[]>
                {
                    Name = "outer-args"
                }
            };

            _helpBuilder.Write(inner);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}outer [<outer-args>...] inner [options] [<inner-args>...]";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_does_not_show_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_not_specified()
        {
            var command = new Command(
                "some-command",
                "Does something");
            command.AddOption(
                new Option("-x", "Indicates whether x"));

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().NotContain("additional arguments");
        }

        [Fact]
        public void Usage_section_does_not_show_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_true()
        {
            var command = new RootCommand();
            var subcommand = new Command("some-command", "Does something");
            command.AddCommand(subcommand);
            subcommand.AddOption(new Option("-x", "Indicates whether x"));
            subcommand.TreatUnmatchedTokensAsErrors = true;

            _helpBuilder.Write(subcommand);

            _console.Out.ToString().Should().NotContain("<additional arguments>");
        }

        [Fact]
        public void Usage_section_shows_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_false()
        {
            var command = new RootCommand();
            var subcommand = new Command("some-command", "Does something");
            command.AddCommand(subcommand);
            subcommand.AddOption(new Option("-x", "Indicates whether x"));
            subcommand.TreatUnmatchedTokensAsErrors = false;

            _helpBuilder.Write(subcommand);

            _console.Out.ToString().Should().Contain("<additional arguments>");
        }

        [Fact]
        public void Usage_section_removes_extra_whitespace()
        {
            var outer = new Command("outer-command", "command help")
            {
                new Argument<string>
                {
                    Name = "outer  args \twith  whitespace"
                },
                new Command("inner-command", "command help")
                {
                    new Argument<string>
                    {
                        Name = "inner-args"
                    }
                }
            };

            new RootCommand().Add(outer);

            _helpBuilder.Write(outer);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} outer-command <outer args with whitespace> [command]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_removes_added_newlines()
        {
            var outer = new Command("outer-command", "command help")
            {
                new Argument<string[]>
                {
                    Name = $"outer args {NewLine}with new{NewLine}lines"
                },
                new Command("inner-command", "command help")
                {
                    new Argument<string>
                    {
                        Name = "inner-args"
                    }
                }
            };

            _helpBuilder.Write(outer);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}outer-command [<outer args with new lines>...] [command]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_properly_wraps_description()
        {
            var helpBuilder = GetHelpBuilder(SmallMaxWidth);

            var outerCommand = new Command("outer-command", "command help")
            {
                new Argument<string[]>
                {
                    Name = "outer args long enough to wrap to a new line"
                },
                new Command("inner-command", "command help")
                {
                    new Argument<string[]>
                    {
                        Name = "inner-args"
                    }
                }
            };

            var rootCommand = new RootCommand
                              {
                                  outerCommand
                              };

            helpBuilder.Write(outerCommand);

            var usageText = $"{_executableName} outer-command [<outer args long enough to wrap to a new line>...] [command]";

            var expectedLines = new List<string> { "Usage:" };
            var builder = new StringBuilder();

            // Don't subtract indentation since we're adding that explicitly
            const int maxWidth = SmallMaxWidth - WindowMargin;

            foreach (var word in usageText.Split())
            {
                var nextLength = word.Length + builder.Length;

                if (nextLength >= maxWidth)
                {
                    expectedLines.Add(builder.ToString());
                    builder.Clear();
                }

                builder.Append(builder.Length == 0 ? $"{_indentation}" : " ");
                builder.Append(word);
            }

            if (builder.Length > 0)
            {
                expectedLines.Add(builder.ToString());
            }

            var expected = string.Join($"{NewLine}", expectedLines);

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_does_not_contain_hidden_argument()
        {
            var commandName = "the-command";
            var visibleArgName = "visible";
            var command = new Command(commandName, "Does things");
            var hiddenArg = new Argument<int>
            {
                Name = "hidden", 
                IsHidden = true
            };
            var visibleArg = new Argument<int>
            {
                Name = visibleArgName,
                IsHidden = false
            };
            command.AddArgument(hiddenArg);
            command.AddArgument(visibleArg);

            _helpBuilder.Write(command);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{commandName} <{visibleArgName}>{NewLine}{NewLine}";
            
            _console.Out.ToString().Should().Contain(expected);
        }

        #endregion Usage

        #region Arguments

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_no_commands_configured()
        {
            _helpBuilder.Write(new RootCommand());

            _console.Out.ToString().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_commands_but_no_arguments_configured()
        {
            var command = new Command("the-command", "command help");

            _helpBuilder.Write(command);
            _console.Out.ToString().Should().NotContain("Arguments:");
            
            _helpBuilder.Write(command);
            _console.Out.ToString().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_included_if_there_are_commands_with_arguments_configured()
        {
            var command = new Command("the-command", "command help")
            {
                new Argument
                {
                    Name = "arg command name",
                    Description = "test",
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_options_with_no_arguments_configured()
        {
            var command = new RootCommand
            {
                new Option(new[] { "-v", "--verbosity" },
                           "Sets the verbosity.")
            };

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_only_options_with_arguments_configured()
        {
            var command = new Command("command")
            {
                new Option("-v", "Sets the verbosity.")
                {
                    Argument = new Argument
                    {
                        Name = "argument for options", Arity = ArgumentArity.ExactlyOne
                    }
                }
            };

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_includes_configured_argument_aliases()
        {
            var command = new Command("the-command", "command help")
            {
                new Option(new[] { "-v", "--verbosity" })
                {
                    Argument = new Argument
                    {
                        Name = "LEVEL", Arity = ArgumentArity.ExactlyOne
                    },
                    Description = "Sets the verbosity."
                }
            };
          
            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-v, --verbosity <LEVEL>");
            help.Should().Contain("Sets the verbosity.");
        }

        [Fact]
        public void Arguments_section_uses_description_if_provided()
        {
            var command = new Command("the-command", "Help text from description")
            {
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "the-arg",
                    Description = "Help text from HelpDetail"
                }
            };

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<the-arg>{_columnPadding}Help text from HelpDetail";

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_does_not_contain_hidden_argument()
        {
            var command = new Command("the-command");
            var hiddenArgName = "the-hidden";
            var hiddenDesc = "the hidden desc";
            var visibleArgName = "the-visible";
            var visibleDesc = "the visible desc";
            var hiddenArg = new Argument<int>
            {
                Name = hiddenArgName,
                Description = hiddenDesc,
                IsHidden = true
            };
            var visibleArg = new Argument<int>
            {
                Name = visibleArgName,
                Description = visibleDesc,
                IsHidden = false
            };
            command.AddArgument(hiddenArg);
            command.AddArgument(visibleArg);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<{visibleArgName}>{_columnPadding}{visibleDesc}{NewLine}{NewLine}";            

            _helpBuilder.Write(command);
            var help = _console.Out.ToString();

            help.Should().Contain(expected);
            help.Should().NotContain(hiddenArgName);
            help.Should().NotContain(hiddenDesc);            
        }

        [Fact]
        public void Arguments_section_does_not_repeat_arguments_that_appear_on_parent_command()
        {
            var reused = new Argument
            {
                Name = "reused",
                Description = "This argument is valid on both outer and inner commands"
            };
            var inner = new Command("inner", "The inner command")
            {
                reused
            };
            var outer = new Command("outer")
            {
                reused,
                inner
            };

            _helpBuilder.Write(inner);

            var help = _console.Out.ToString();

            help.Should()
                .Be($"inner:{NewLine}" +
                         $"  The inner command{NewLine}" +
                         $"{NewLine}" +
                         $"Usage:{NewLine}" +
                         $"  outer [<reused>] inner [<reused>]{NewLine}" +
                         $"{NewLine}" +
                         $"Arguments:{NewLine}" +
                         $"  <reused>{_columnPadding}This argument is valid on both outer and inner commands{NewLine}{NewLine}");
        }

        [Fact]
        public void Arguments_section_aligns_arguments_on_new_lines()
        {
            var inner = new Command("inner", "HelpDetail text for the inner command")
            {
                new Argument<string>
                {
                    Name = "the-inner-command-arg",
                    Description = "The argument for the inner command",
                }
            };
            var outer = new Command("outer", "HelpDetail text for the outer command")
            {
                new Argument<string>
                {
                    Name = "outer-command-arg", Description = "The argument for the outer command"
                },
                inner
            };

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>    {_columnPadding}The argument for the outer command{NewLine}" +
                $"{_indentation}<the-inner-command-arg>{_columnPadding}The argument for the inner command";

            _helpBuilder.Write(inner);

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_removes_extra_whitespace()
        {
            var outer = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "outer-command-arg",
                    Description = "Argument\tfor the   inner command",
                }
            };

            _helpBuilder.Write(outer);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>{_columnPadding}Argument for the inner command{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_removes_added_newlines()
        {
            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Name = "outer-command-arg",
                    Description = $"The argument{NewLine}for the{NewLine}inner command",
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            _helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>{_columnPadding}The argument for the inner command{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_properly_wraps_description()
        {
            var longCmdText =
                $"Argument{NewLine}" +
                $"for inner command with line breaks that is long enough to wrap to a" +
                $"{NewLine}new line";

            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Name = "outer-command-arg",
                    Description = longCmdText
                }
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>{_columnPadding}Argument for inner command with line breaks{NewLine}" +
                $"{_indentation}                   {_columnPadding}that is long enough to wrap to a new line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(bool?))]
        public void Command_argument_descriptor_is_empty_for_boolean_values(Type type)
        {
            var description = "This is the argument description";

            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Description = description,
                    ArgumentType = type
                }
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}{_columnPadding}{description}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Theory]
        [InlineData(typeof(FileAccess))]
        [InlineData(typeof(FileAccess?))]
        public void Command_argument_descriptor_indicates_enums_values(Type type)
        {
            var description = "This is the argument description";

            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Description = description,
                    ArgumentType = type
                }
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<Read|ReadWrite|Write>{_columnPadding}{description}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(bool?))]
        public void Option_argument_descriptor_is_empty_for_boolean_values(Type type)
        {
            var description = "This is the option description";

            var command = new Command(
                "outer", "Help text for the outer command")
                          {
                              new Option("--opt", description)
                              {
                                  Argument = new Argument
                                             {
                                                 Description = description,
                                                 ArgumentType = type
                                             }
                              }
                          };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);
            
            _console.Out.ToString().Should().Contain($"--opt{_columnPadding}{description}");
        }
        
        [Theory]
        [InlineData(typeof(FileAccess))]
        [InlineData(typeof(FileAccess?))]
        public void Option_argument_descriptor_indicates_enums_values(Type type)
        {
            var description = "This is the argument description";

            var command = new Command(
                              "outer", "Help text for the outer command")
                          {
                              new Option("--opt", description)
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = type
                                             }
                              }
                          };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain($"--opt <Read|ReadWrite|Write>{_columnPadding}{description}");
        }

        #endregion Arguments

        #region Options

        [Fact]
        public void Options_section_is_not_included_if_no_options_configured()
        {
            var commandLineBuilder = new CommandLineBuilder()
                                     .AddCommand(new Command("outer", "description for outer"))
                                     .Command;

            _helpBuilder.Write(commandLineBuilder);

            _console.Out.ToString().Should().NotContain("Options:");
        }

        [Fact]
        public void Options_section_is_not_included_if_only_subcommands_configured()
        {
            var command = new Command("outer", "description for outer");
            command.AddCommand(new Command("inner"));

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().NotContain("Options:");
        }

        [Fact]
        public void Options_section_includes_option_with_empty_description()
        {
            var command = new Command("the-command", "Does things.")
                          {
                              new Option("-x"),
                              new Option("-n")
                          };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-x");
            help.Should().Contain("-n");
        }

        [Fact]
        public void Options_section_does_not_contain_option_with_HelpDefinition_that_IsHidden()
        {
            var command = new Command("the-command");
            command.AddOption(new Option("-x", "Is Hidden")
            {
                IsHidden = true
            });
            command.AddOption(new Option("-n", "Not Hidden")
            {
                IsHidden = false
            });
            

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-n");
            help.Should().Contain("Not Hidden");
            help.Should().NotContain("-x");
            help.Should().NotContain("Is hidden");
        }

        [Fact]
        public void Options_section_aligns_options_on_new_lines()
        {
            var command = new Command(
                              "the-command",
                              "Help text for the command")
                          {
                              new Option(new[] { "-a", "--aaa" },
                                         "An option with 8 characters"),
                              new Option(new[] { "-b", "--bbbbbbbbbb" },
                                         "An option with 15 characters")
                          };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            var lines = help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("-a"));
            var optionB = lines.Last(line => line.Contains("-b"));

            optionA.IndexOf("An option", StringComparison.Ordinal)
                .Should()
                .Be(optionB.IndexOf("An option", StringComparison.Ordinal));
        }

        [Fact]
        public void Retains_single_dash_on_multi_char_option()
        {
            var command = new Command("command", "Help Test")
                          {
                              new Option(
                                  new[] { "-multi", "--alt-option" },
                                  "HelpDetail for option")
                          };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-multi");
            help.Should().NotContain("--multi");
        }

        [Fact]
        public void Options_section_retains_multiple_dashes_on_single_char_option()
        {
            var command = new Command("command", "Help Test")
                          {
                              new Option(
                                  new[] { "--m", "--alt-option" },
                                  "HelpDetail for option")
                          };

            _helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain("--m");
        }

        [Fact]
        public void Options_section_removes_extra_whitespace()
        {
            var command = new Command("test-command", "Help text for the command")
                          {
                              new Option(
                                  new[] { "-a", "--aaa" },
                                  "Help   for      the   option")
                          };

            _helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_removes_added_newlines()
        {
            var command =
                new Command(
                    "test-command",
                    "Help text for the command")
                {
                    new Option(
                        new[] { "-a", "--aaa" },
                        $"Help{NewLine}for {NewLine} the{NewLine}option")
                };

            _helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_properly_wraps_description()
        {
            var longOptionText =
                $"The option{NewLine}" +
                $"with line breaks that is long enough to wrap to a{NewLine}" +
                $"new line";

            var command = new Command("test-command",
                                      "Help text for the command")
                          {
                              new Option(
                                  new[] { "-a", "--aaa" },
                                  longOptionText)
                          };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}-a, --aaa{_columnPadding}The option with line breaks that is long enough to{NewLine}" +
                $"{_indentation}         {_columnPadding}wrap to a new line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_does_not_contain_hidden_argument()
        {
            var command = new Command("the-command", "Does things.");
            var opt1 = new Option("option1")
            {
                Argument = new Argument<int>()
                {
                    Name = "the-hidden",
                    IsHidden = true
                }
            };
            var opt2 = new Option("option2")
            {
                Argument = new Argument<int>()
                {
                    Name = "the-visible",
                    IsHidden = false
                }
            };
            command.AddOption(opt1);
            command.AddOption(opt2);

            _helpBuilder.Write(command);
            var help = _console.Out.ToString();

            help.Should().NotContain("the-hidden");
            help.Should().Contain("the-visible");
        }

        [Fact]
        public void Required_options_are_indicated()
        {
            var command = new RootCommand
            {
                new Option("--required")
                {
                    Required = true
                }
            };

            _helpBuilder.Write(command);
            
            var help = _console.Out.ToString();

            help.Should()
                .Contain("--required (REQUIRED)");
        }
        
        [Fact]
        public void Required_options_are_indicated_when_argument_is_named()
        {
            var command = new RootCommand
            {
                new Option(new[] {"-r", "--required" })
                {
                    Required = true,
                    Argument = new Argument<string>("ARG")
                }
            };

            _helpBuilder.Write(command);
            
            var help = _console.Out.ToString();

            help.Should()
                .Contain("-r, --required <ARG> (REQUIRED)");
        }

        #endregion Options

        #region Subcommands

        [Fact]
        public void Subcommand_help_does_not_include_names_of_sibling_commands()
        {
            var inner = new Command("inner", "inner description")
                        {
                            new Command(
                                "inner-er", "inner-er description")
                            {
                                new Option("some-option",
                                           "some-option description")
                            }
                        };

            var sibling = new Command("sibling", "sibling description");

            var outer = new Command("outer", "outer description")
                        {
                            sibling,
                            inner
                        };

            _helpBuilder.Write(inner);

            _console.Out.ToString().Should().NotContain("sibling");
        }

        [Fact]
        public void Subcommands_remove_extra_whitespace()
        {
            var command = new Command("outer", "outer command help")
            {
                new Argument<string[]>
                {
                    Name = "outer-args"
                },
                new Command("inner", "inner    command\t help  with whitespace")
                {
                    new Argument<string[]>
                    {
                        Name = "inner-args"
                    }
                }
            };

            _helpBuilder.Write(command);

            var expected =
                $"Commands:{NewLine}" +
                $"{_indentation}inner <inner-args>{_columnPadding}inner command help with whitespace{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_remove_added_newlines()
        {
            var command = new Command("outer", "outer command help")
                {
                    new Argument<string>
                    {
                        Name = "outer-args"
                    },
                    new Command("inner", $"inner{NewLine}command help {NewLine} with {NewLine}newlines")
                    {
                        new Argument<string>

                        {
                            Name = "inner-args"
                        }
                    }
                };

            _helpBuilder.Write(command);

            var expected =
                $"Commands:{NewLine}" +
                $"{_indentation}inner <inner-args>{_columnPadding}inner command help with newlines{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_properly_wraps_description()
        {
            var longSubcommandText =
                $"The{NewLine}" +
                $"subcommand with line breaks that is long enough to wrap to a{NewLine}" +
                $"new line";

            var helpBuilder = GetHelpBuilder(SmallMaxWidth);

            var command = new Command("outer-command", "outer command help")
             {
                 new Argument<string[]>
                 {
                     Name = "outer-args"
                 },
                 new Command("inner-command", longSubcommandText)
                 {
                     new Argument<string[]>
                     {
                         Name = "inner-args"
                     },
                     new Option(new[]
                     {
                         "-v",
                         "--verbosity"
                     })
                 }
             };

             helpBuilder.Write(command);

            var expected =
                $"Commands:{NewLine}" +
                $"{_indentation}inner-command <inner-args>{_columnPadding}The subcommand with line breaks that{NewLine}" +
                $"{_indentation}                          {_columnPadding}is long enough to wrap to a new line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Subcommand_help_contains_command_with_empty_description()
        {
            var command = new Command("the-command", "Does things.");
            var subCommand = new Command("the-subcommand", description: null);
            command.AddCommand(subCommand);

            _helpBuilder.Write(command);
            var help = _console.Out.ToString();

            help.Should().Contain("the-subcommand");
        }

        [Fact]
        public void Subcommand_help_does_not_contain_hidden_command()
        {
            var command = new Command("the-command", "Does things.");
            var hiddenSubCommand = new Command("the-hidden")
            {
                IsHidden = true
            };
            var visibleSubCommand = new Command("the-visible")
            {
                IsHidden = false
            };
            command.AddCommand(hiddenSubCommand);
            command.AddCommand(visibleSubCommand);

            _helpBuilder.Write(command);
            var help = _console.Out.ToString();

            help.Should().NotContain("the-hidden");
            help.Should().Contain("the-visible");
        }

        [Fact]
        public void Subcommand_help_does_not_contain_hidden_argument()
        {
            var command = new Command("the-command", "Does things.");
            var subCommand = new Command("the-subcommand");
            var hidden = new Argument<int>()
            {
                Name = "the-hidden", 
                IsHidden = true
            };
            var visible = new Argument<int>()
            {
                Name = "the-visible", 
                IsHidden = false
            };
            subCommand.AddArgument(hidden);
            subCommand.AddArgument(visible);
            command.AddCommand(subCommand);

            _helpBuilder.Write(command);
            var help = _console.Out.ToString();

            help.Should().NotContain("the-hidden");
            help.Should().Contain("the-visible");
        }

        #endregion Subcommands

        [Fact]
        public void Help_text_can_be_added_after_default_text_by_inheriting_HelpBuilder()
        {
            var parser = new CommandLineBuilder()
                         .UseDefaults()
                         .UseHelpBuilder(context => new CustomHelpBuilderThatAddsTextAfterDefaultText(context.Console, "The text to add"))
                         .Build();

            var console = new TestConsole();

            parser.Invoke("-h", console);

            console.Out.ToString().Should().EndWith("The text to add");
        }

        private class CustomHelpBuilderThatAddsTextAfterDefaultText : HelpBuilder
        {
            private readonly string _theTextToAdd;

            public CustomHelpBuilderThatAddsTextAfterDefaultText(IConsole console, string theTextToAdd) : base(console)
            {
                _theTextToAdd = theTextToAdd;
            }

            public override void Write(ICommand command)
            {
                base.Write(command);
                base.Console.Out.Write(_theTextToAdd);
            }
        }
    }
}
