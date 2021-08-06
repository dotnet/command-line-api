// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public partial class HelpBuilderTests
    {
        private const int SmallMaxWidth = 70;
        private const int LargeMaxWidth = 200;
        private const int ColumnGutterWidth = 2;
        private const int IndentationWidth = 2;

        private readonly HelpBuilder _helpBuilder;
        private readonly TestConsole _console;
        private readonly string _executableName;
        private readonly string _columnPadding;
        private readonly string _indentation;

        public HelpBuilderTests()
        {
            _console = new TestConsole();
            _helpBuilder = GetHelpBuilder(LargeMaxWidth);
            _columnPadding = new string(' ', ColumnGutterWidth);
            _indentation = new string(' ', IndentationWidth);
            _executableName = RootCommand.ExecutableName;
        }

        private HelpBuilder GetHelpBuilder(int maxWidth)
        {
            return new HelpBuilder(
                console: _console,
                Resources.Instance,
                maxWidth
            );
        }

        #region Synopsis

        [Fact]
        public void Synopsis_section_keeps_added_newlines()
        {
            var command = new RootCommand(
                $"test{NewLine}\r\ndescription with\nline breaks");

            _helpBuilder.Write(command);

            var expected =
                $"{_indentation}test{NewLine}" +
                $"{_indentation}{NewLine}" +
                $"{_indentation}description with{NewLine}" +
                $"{_indentation}line breaks{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Synopsis_section_properly_wraps_description()
        {
            var longSynopsisText =
                $"test\t" +
                $"description with some tabs that is long enough to wrap to a\t" +
                $"new line";

            var command = new RootCommand(description: longSynopsisText);

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"{_indentation}test\tdescription with some tabs that is long enough to wrap to a\t{NewLine}" +
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

            new HelpBuilder(_console, Resources.Instance, LargeMaxWidth).Write(command);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} [options] the-command {expectedDescriptor}";

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
                new Option(new[] { "-v", "--verbosity" }, "Sets the verbosity")
            };

            var rootCommand = new RootCommand();
            rootCommand.AddCommand(command);

            _helpBuilder.Write(command);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} [options] the-command {expectedDescriptor}";

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
                $"{_indentation}{_executableName} [options] outer inner inner-er";

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
            _ = new Command("outer", "command help")
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
                $"{_indentation}outer [options] [<outer-args>...] inner [<inner-args>...]";

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
        public void Usage_section_keeps_added_newlines()
        {
            var outer = new Command("outer-command", "command help")
            {
                new Argument<string[]>
                {
                    Name = $"outer args {NewLine}\r\nwith new\nlines"
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
                $"{_indentation}outer-command [<outer args {NewLine}" +
                $"{_indentation}{NewLine}" +
                $"{_indentation}with new{NewLine}" +
                $"{_indentation}lines>...] [command]{NewLine}{NewLine}";

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
            //NB: Using Command with a fixed name, rather than RootCommand here
            //because RootCommand.ExecutableName returns different values when
            //run under net5 vs net462
            _ = new Command("System.CommandLine")
            {
                outerCommand
            };

            helpBuilder.Write(outerCommand);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}System.CommandLine outer-command [<outer args long enough to wrap {NewLine}" +
                $"{_indentation}to a new line>...] [command]{NewLine}{NewLine}";

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

            string help = _console.Out.ToString();
            help.Should().Contain(expected);
            help.Should().NotContain("hidden");
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
                    Description = "test"
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
                new Option("-v", "Sets the verbosity.", arity: ArgumentArity.ExactlyOne)
                {
                    ArgumentHelpName = "argument for options"
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
                new Option(new[] { "-v", "--verbosity" }, arity: ArgumentArity.ExactlyOne)
                {
                    ArgumentHelpName = "LEVEL",
                    Description = "Sets the verbosity."
                }
            };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-v, --verbosity <LEVEL>");
            help.Should().Contain("Sets the verbosity.");
        }


        private enum VerbosityOptions
        {
            q,
            m,
            n,
            d,
        }

        [Fact]
        public void Arguments_section_uses_name_over_suggestions_if_specified()
        {
            var command = new Command("the-command")
            {
                new Option<VerbosityOptions>(new[] { "-v", "--verbosity" })
                {
                    ArgumentHelpName = "LEVEL"
                }
            };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();
            help.Should().Contain("-v, --verbosity <LEVEL>");
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
            _ = new Command("outer")
            {
                reused,
                inner
            };

            _helpBuilder.Write(inner);

            var help = _console.Out.ToString();

            help.Should().Contain($"Arguments:{NewLine}" +
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
            _ = new Command("outer", "HelpDetail text for the outer command")
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
        public void Arguments_section_keeps_added_newlines()
        {
            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Name = "outer-command-arg",
                    Description = $"The argument\r\nfor the\ninner command",
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            _helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>{_columnPadding}The argument{NewLine}" +
                $"{_indentation}                   {_columnPadding}for the{NewLine}" +
                $"{_indentation}                   {_columnPadding}inner command{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_keeps_added_newlines_when_width_is_very_small()
        {
            var command = new Command("outer", "Help text for the outer command")
            {
                new Argument
                {
                    Name = "outer-command-arg",
                    Description = $"The argument\r\nfor the\ninner command",
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            var helpBuilder = GetHelpBuilder(25);

            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-com{_columnPadding}The {NewLine}" +
                $"{_indentation}mand-arg> {_columnPadding}argument{NewLine}" +
                $"{_indentation}          {_columnPadding}for the{NewLine}" +
                $"{_indentation}          {_columnPadding}inner {NewLine}" +
                $"{_indentation}          {_columnPadding}command{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_properly_wraps_description()
        {
            var longCmdText =
                $"Argument\t" +
                $"for inner command with some tabs that is long enough to wrap to a\t" +
                $"new line";

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
                $"{_indentation}<outer-command-arg>{_columnPadding}Argument\tfor inner command with some tabs that {NewLine}" +
                $"{_indentation}                   {_columnPadding}is long enough to wrap to a\tnew line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_properly_wraps()
        {
            var name = "argument-name-for-a-command-that-is-long-enough-to-wrap-to-a-new-line";
            var description = "Argument description for a command with line breaks that is long enough to wrap to a new line.";

            var command = new RootCommand
            {
                new Argument
                {
                    Name = name,
                    Description = description
                }
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<argument-name-for-a-command-that{_columnPadding}Argument description for a {NewLine}" +
                $"{_indentation}-is-long-enough-to-wrap-to-a-new-{_columnPadding}command with line breaks that is {NewLine}" +
                $"{_indentation}line>                            {_columnPadding}long enough to wrap to a new {NewLine}" +
                $"{_indentation}                                 {_columnPadding}line.{NewLine}{NewLine}";

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
                              new Option("--opt", description, argumentType: type)
                          };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain($"--opt{_columnPadding}{description}");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1157
        public void Command_arguments_show_argument_name_as_descriptor()
        {
            var command = new RootCommand
            {
                new Argument<bool>("boolArgument", "Some value"),
                new Argument<int>("intArgument", "Another value"),
            };
            
            var helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<boolArgument>{_columnPadding}Some value{NewLine}" +
                $"{_indentation}<intArgument> {_columnPadding}Another value{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
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
                              new Option("--opt", description, argumentType: type)
                          };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            _console.Out.ToString().Should().Contain($"--opt <Read|ReadWrite|Write>{_columnPadding}{description}");
        }

        [Fact]
        public void Help_describes_default_value_for_defaultable_argument()
        {
            var argument = new Argument
            {
                Name = "the-arg",
                Description = "Help text from HelpDetail",
            };
            argument.SetDefaultValue("the-arg-value");

            var command = new Command("the-command",
                "Help text from description") { argument };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().Contain($"[default: the-arg-value]");
        }

        [Fact]
        public void Command_arguments_default_value_provided()
        {
            var argument = new Argument
            {
                Name = "the-arg",
            };

            var otherArgument = new Argument
            {
                Name = "the-other-arg",
            };
            argument.SetDefaultValue("the-arg-value");
            otherArgument.SetDefaultValue("the-other-arg-value");
            var command = new Command("the-command",
                "Help text from description")
            {
                argument,
                otherArgument
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var help = _console.Out.ToString();

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<the-arg>      {_columnPadding}[default: the-arg-value]{NewLine}" +
                $"{_indentation}<the-other-arg>{_columnPadding}[default: the-other-arg-value]{NewLine}";

            help.Should().Contain(expected);
        }

        [Fact]
        public void Command_arguments_with_default_values_that_are_enumerable_display_pipe_delimited_list()
        {
            var command = new Command("the-command", "command help")
            {
                new Argument<List<int>>("filter-size",
                    getDefaultValue: () => new List<int>() { 0, 2, 4 })
            };

            _helpBuilder.Write(command);
            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<filter-size>{_columnPadding}[default: 0|2|4]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Command_arguments_can_customize_default_value()
        {
            var argument = new Argument<string>("some-arg", getDefaultValue: () => "not 42");
            var command = new Command("the-command", "command help")
            {
                argument
            };

            _helpBuilder.Customize(argument, defaultValue: "42");

            _helpBuilder.Write(command);
            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<some-arg>{_columnPadding}[default: 42]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Command_arguments_can_customize_descriptor()
        {
            var argument = new Argument<string>("some-arg", getDefaultValue: () => "not 42");
            var command = new Command("the-command", "command help")
            {
                argument
            };

            _helpBuilder.Customize(argument, descriptor: "some-other-arg");

            _helpBuilder.Write(command);
            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}some-other-arg{_columnPadding}[default: not 42]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Null_argument_on_customize_throws()
        {
            Action action = () => _helpBuilder.Customize((IArgument)null);
            action.Should().Throw<ArgumentNullException>();
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
        public void Options_section_keeps_added_newlines()
        {
            var command =
                new Command(
                    "test-command",
                    "Help text for the command")
                {
                    new Option(
                        new[] { "-a", "--aaa" },
                        $"Help{NewLine}for \r\n the\noption")
                };

            _helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}-a, --aaa{_columnPadding}Help{NewLine}" +
                $"{_indentation}         {_columnPadding}for {NewLine}" +
                $"{_indentation}         {_columnPadding} the{NewLine}" +
                $"{_indentation}         {_columnPadding}option{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_properly_wraps_description()
        {
            var longOptionText =
                $"The option\t" +
                $"with some tabs that is long enough to wrap to a\t" +
                $"new line";

            var command = new Command("test-command", "Help text for the command")
            {
                new Option(
                    new[] { "-a", "--aaa" },
                    longOptionText)
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}-a, --aaa{_columnPadding}The option\twith some tabs that is long enough to wrap to {NewLine}" +
                $"{_indentation}         {_columnPadding}a\tnew line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_properly_wraps()
        {
            var alias = "--option-alias-for-a-command-that-is-long-enough-to-wrap-to-a-new-line";
            var description = "Option description that is long enough to wrap.";

            var command = new RootCommand
            {
                new Option(alias, description)
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}--option-alias-for-a-command-that{_columnPadding}Option description that is long {NewLine}" +
                $"{_indentation}-is-long-enough-to-wrap-to-a-new-{_columnPadding}enough to wrap.{NewLine}" +
                $"{_indentation}line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Required_options_are_indicated()
        {
            var command = new RootCommand
            {
                new Option("--required")
                {
                    IsRequired = true
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
                new Option<string>(new[] {"-r", "--required" })
                {
                    IsRequired = true,
                    ArgumentHelpName = "ARG"
                }
            };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should()
                .Contain("-r, --required <ARG> (REQUIRED)");
        }

        [Fact]
        public void Help_option_is_shown_in_help()
        {
            var parser = new CommandLineBuilder()
                         .UseHelp()
                         .Build();

            _helpBuilder.Write(parser.Configuration.RootCommand);

            var help = _console.Out.ToString();

            help.Should()
                .Contain($"-?, -h, --help{_columnPadding}Show help and usage information");
        }

        [Fact]
        public void Options_aliases_differing_only_by_prefix_are_deduplicated_favoring_dashed_prefixes()
        {
            var command = new RootCommand
            {
                new Option(new[] { "-x", "/x" })
            };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().NotContain("/x");
        }

        [Fact]
        public void Options_aliases_differing_only_by_prefix_are_deduplicated_favoring_double_dashed_prefixes()
        {
            var command = new RootCommand
            {
                new Option(new[] { "--long", "/long" })
            };

            _helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().NotContain("/long");
        }

        [Fact]
        public void Options_help_preserves_the_order_options_are_added_the_the_parent_command()
        {
            var command = new RootCommand
            {
                new Option(new[] { "--first", "-f" }),
                new Option(new[] { "--second", "-s" }),
                new Option(new[] { "--third" }),
                new Option(new[] { "--last", "-l" })
            };

            _helpBuilder.Write(command);
            var help = _console
                       .Out
                       .ToString()
                       .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim());

            help.Should().ContainInOrder(
                "-f, --first",
                "-s, --second",
                "--third",
                "-l, --last");
        }

        [Fact]
        public void Option_aliases_are_shown_before_long_names_regardless_of_alphabetical_order()
        {
            var command = new RootCommand
            {
                new Option(new[] { "-z", "-a", "--zzz", "--aaa" })
            };

            _helpBuilder.Write(command);

            _console
                .Out
                .ToString().Should().Contain("-a, -z, --aaa, --zzz");
        }

        [Fact]
        public void Help_describes_default_value_for_option_with_argument_having_default_value()
        {
            var command = new Command("the-command", "command help")
            {
                new Option(new[] { "-arg"}, getDefaultValue: () => "the-arg-value")
                {
                    ArgumentHelpName = "the-arg"
                }
            };

            HelpBuilder helpBuilder = GetHelpBuilder(SmallMaxWidth);

            helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().Contain($"[default: the-arg-value]");
        }
        
        [Fact]
        public void Option_arguments_with_default_values_that_are_enumerable_display_pipe_delimited_list()
        {
            var command = new Command("the-command", "command help")
            {
                new Option<List<int>>(
                    "--filter-size",
                    getDefaultValue: () => new List<int>() { 0, 2, 4 })
                { }
            };

            _helpBuilder.Write(command);
            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}--filter-size <filter-size>{_columnPadding}[default: 0|2|4]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Option_arguments_with_default_values_that_are_array_display_pipe_delimited_list()
        {
            var command = new Command("the-command", "command help")
            {
                new Option<string[]>(
                    "--prefixes",
                    getDefaultValue: () => new[]{ "^(TODO|BUG)", "^HACK" })
                { }
            };

            _helpBuilder.Write(command);
            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}--prefixes <prefixes>{_columnPadding}[default: ^(TODO|BUG)|^HACK]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Option_can_customize_default_value()
        {
            var option = new Option<string>("--the-option", getDefaultValue: () => "not 42");
            var command = new Command("the-command", "command help")
            {
                option
            };

            _helpBuilder.Customize(option, defaultValue: "42");
            
            _helpBuilder.Write(command);
            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}--the-option <the-option>{_columnPadding}[default: 42]{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Option_can_customize_descriptor()
        {
            var option = new Option<string>("--the-option", "option description");
            var command = new Command("the-command", "command help")
            {
                option
            };

            _helpBuilder.Customize(option, descriptor: "other-name");

            _helpBuilder.Write(command);
            var expected =
                $"Options:{NewLine}" +
                $"{_indentation}other-name{_columnPadding}option description{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Null_option_on_customize_throws()
        {
            Action action = () => _helpBuilder.Customize((IOption)null);
            action.Should().Throw<ArgumentNullException>();
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
        public void Subcommands_keep_added_newlines()
        {
            var command = new Command("outer", "outer command help")
            {
                new Argument<string>
                {
                    Name = "outer-args"
                },
                new Command("inner", $"inner{NewLine}command help \r\n with \nnewlines")
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
                $"{_indentation}inner <inner-args>{_columnPadding}inner{NewLine}" +
                $"{_indentation}                  {_columnPadding}command help {NewLine}" +
                $"{_indentation}                  {_columnPadding} with {NewLine}" +
                $"{_indentation}                  {_columnPadding}newlines{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_properly_wraps_description()
        {
            var longSubcommandDescription =
                $"The\t" +
                $"subcommand with some tabs that is long enough to wrap to a\t" +
                $"new line";

            var helpBuilder = GetHelpBuilder(SmallMaxWidth);

            var command = new Command("outer-command", "outer command help")
            {
                new Argument<string[]>
                {
                    Name = "outer-args"
                },
                new Command("inner-command", longSubcommandDescription)
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
                $"{_indentation}inner-command <inner-args>{_columnPadding}The\tsubcommand with some tabs that is {NewLine}" +
                $"{_indentation}                          {_columnPadding}long enough to wrap to a\tnew line{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_section_properly_wraps()
        {
            var name = "subcommand-name-that-is-long-enough-to-wrap-to-a-new-line";
            var description = "Subcommand description that is really long. So long that it caused the line to wrap.";

            var command = new RootCommand
            {
                new Command(name, description)
            };

            var helpBuilder = GetHelpBuilder(SmallMaxWidth);
            helpBuilder.Write(command);

            var expected =
                $"Commands:{NewLine}" +
                $"{_indentation}subcommand-name-that-is-long-enou{_columnPadding}Subcommand description that is {NewLine}" +
                $"{_indentation}gh-to-wrap-to-a-new-line         {_columnPadding}really long. So long that it {NewLine}" +
                $"{_indentation}                                 {_columnPadding}caused the line to wrap.{NewLine}{NewLine}";

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

        [Fact]
        public void Subcommand_can_customize_descriptor()
        {
            var subcommand = new Command("subcommand", "subcommand description");
            var command = new Command("the-command", "command help")
            {
                subcommand
            };

            _helpBuilder.Customize(subcommand, descriptor: "other-name");

            _helpBuilder.Write(command);
            var expected =
                $"Commands:{NewLine}" +
                $"{_indentation}other-name{_columnPadding}subcommand description{NewLine}{NewLine}";

            _console.Out.ToString().Should().Contain(expected);
        }

        [Fact]
        public void Null_command_on_customize_throws()
        {
            Action action = () => _helpBuilder.Customize((ICommand)null);
            action.Should().Throw<ArgumentNullException>();
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

        [Fact]
        public void Help_describes_default_value_for_subcommand_with_arguments_and_only_defaultable_is_shown()
        {
            var argument = new Argument
            {
                Name = "the-arg",
            };
            var otherArgumentHidden = new Argument
            {
                Name = "the-other-hidden-arg",
                IsHidden = true
            };
            argument.SetDefaultValue("the-arg-value");
            otherArgumentHidden.SetDefaultValue("the-other-hidden-arg-value");

            var command = new Command("outer", "outer command help")
                {
                    new Argument<string>
                    {
                        Name = "outer-args"
                    },
                    new Command("inner", $"inner command help")
                    {
                        argument,
                        otherArgumentHidden,
                        new Argument<string>
                        {
                            Name = "inner-other-arg-no-default"
                        }
                    }
                };

            HelpBuilder helpBuilder = GetHelpBuilder(LargeMaxWidth);

            helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().Contain($"[default: the-arg-value]");
        }

        [Fact]
        public void Help_describes_default_values_for_subcommand_with_multiple_defaultable_arguments()
        {
            var argument = new Argument
            {
                Name = "the-arg",
            };
            var otherArgument = new Argument
            {
                Name = "the-other-arg"
            };
            argument.SetDefaultValue("the-arg-value");
            otherArgument.SetDefaultValue("the-other-arg-value");

            var command = new Command("outer", "outer command help")
                {
                    new Argument<string>
                    {
                        Name = "outer-args"
                    },
                    new Command("inner", $"inner command help")
                    {
                        argument, otherArgument
                    }
                };

            HelpBuilder helpBuilder = GetHelpBuilder(LargeMaxWidth);

            helpBuilder.Write(command);

            var help = _console.Out.ToString();

            help.Should().Contain($"[the-arg: the-arg-value, the-other-arg: the-other-arg-value]");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Constructor_ignores_non_positive_max_width(int maxWidth)
        {
            var helpBuilder = new HelpBuilder(_console, Resources.Instance, maxWidth);
            Assert.Equal(int.MaxValue, helpBuilder.MaxWidth);
        }

        private class CustomHelpBuilderThatAddsTextAfterDefaultText : HelpBuilder
        {
            private readonly string _theTextToAdd;

            public CustomHelpBuilderThatAddsTextAfterDefaultText(IConsole console, string theTextToAdd) 
                : base(console, Resources.Instance)
            {
                _theTextToAdd = theTextToAdd;
            }

            public override void Write(ICommand command)
            {
                base.Write(command);
                Console.Out.Write(_theTextToAdd);
            }
        }
    }
}
