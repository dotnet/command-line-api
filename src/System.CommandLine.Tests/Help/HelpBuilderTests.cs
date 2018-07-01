// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public class HelpBuilderTests
    {
        private const int SmallMaxWidth = 70;
        private const int LargeMaxWidth = 200;
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
            _executableName = CommandLineBuilder.ExeName;
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

        private string GetHelpText()
        {
            var helpText = _console.Out.ToString();
            _console.Out.Flush();
            return helpText;
        }

        #region Synopsis

        [Fact]
        public void Synopsis_section_removes_extra_whitespace()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = "test  description\tfor synopsis",
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description for synopsis{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Synopsis_section_removes_added_newlines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = $"test{NewLine}description with{NewLine}line breaks",
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description with line breaks{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Synopsis_section_properly_wraps_description()
        {
            var longSynopsisText =
            $"test{NewLine}" +
            $"description with line breaks that is long enough to wrap to a{NewLine}" +
            $"new line";

            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = GetHelpBuilder(SmallMaxWidth),
                    Description = longSynopsisText,
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description with line breaks that is long enough to wrap to a{NewLine}" +
            $"{_indentation}new line{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        #endregion Synopsis

        #region Usage

        [Fact]
        public void Usage_section_shows_arguments_if_there_are_arguments_for_command()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "command help",
                arguments: args => args
                    .WithHelp(name: "the-args")
                    .ZeroOrMore(),
                symbols: cmd => cmd.AddOption(
                    new[] { "-v", "--verbosity" },
                    "Sets the verbosity"))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var expected =
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} the-command [options] <the-args>";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_for_subcommand_shows_names_of_parent_commands()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand(
                    "outer", "the outer command",
                    outer => outer.AddCommand(
                        "inner", "the inner command",
                        inner => inner.AddCommand(
                            "inner-er", "the inner-er command",
                            innerEr => innerEr.AddOption(
                                "--some-option",
                                "some option"))))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .Subcommand("inner")
                .Subcommand("inner-er")
                .WriteHelp(_console);

            var expected =
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer inner inner-er [options]";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_for_subcommand_shows_arguments_for_subcommand_and_parent_command()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand(
                "outer-command", "command help",
                arguments: outerArgs => outerArgs
                    .WithHelp(name: "outer-args")
                    .ZeroOrMore(),
                symbols: outer => outer.AddCommand(
                    "inner-command", "command help",
                    arguments: args => args
                        .WithHelp(name: "inner-args")
                        .ZeroOrOne(),
                symbols: inner => inner.AddOption(
                    "-v|--verbosity",
                    "Sets the verbosity")))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command [options] <inner-args>";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_does_not_show_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_not_specified()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .TreatUnmatchedTokensAsErrors(true)
            .AddCommand("some-command", "Does something",
                c => c.AddOption(
                    "-x",
                    "Indicates whether x"))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("some-command")
                .WriteHelp(_console);

            GetHelpText().Should().NotContain("additional arguments");
        }

        [Fact]
        public void Usage_section_does_not_show_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_true()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("some-command", "Does something",
                    c => c.AddOption(
                        "-x",
                        "Indicates whether x"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("some-command")
                .WriteHelp(_console);

            GetHelpText().Should().NotContain("additional arguments");
        }

        [Fact]
        public void Usage_section_shows_additional_arguments_when_TreatUnmatchedTokensAsErrors_is_false()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .TreatUnmatchedTokensAsErrors(false)
                .AddCommand("some-command", "Does something",
                    c => c.AddOption(
                        "-x",
                        "Indicates whether x"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("some-command")
                .WriteHelp(_console);

            var expected =
                $"Usage:{NewLine}" +
                $"{_indentation}{_executableName} some-command [options] [[--] <additional arguments>...]]";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_removes_extra_whitespace()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = GetHelpBuilder(SmallMaxWidth),
            }
            .AddCommand(
                "outer-command", "command help",
                arguments: outerArgs => outerArgs
                    .WithHelp(name: $"outer  args \twith  whitespace")
                    .ZeroOrMore(),
                symbols: outer => outer.AddCommand(
                    "inner-command", "command help",
                    arguments: args => args
                        .WithHelp(name: "inner-args")
                        .ZeroOrOne()))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var expected =
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer args with whitespace> [command]{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_removes_added_newlines()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand(
                "outer-command", "command help",
                arguments: outerArgs => outerArgs
                    .WithHelp(name: $"outer args {NewLine}with new{NewLine}lines")
                    .ZeroOrMore(),
                symbols: outer => outer.AddCommand(
                    "inner-command", "command help",
                    arguments: args => args
                        .WithHelp(name: "inner-args")
                        .ZeroOrOne()))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var expected =
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer args with new lines> [command]{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Usage_section_properly_wraps_description()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = GetHelpBuilder(SmallMaxWidth),
            }
            .AddCommand(
                "outer-command", "command help",
                arguments: outerArgs => outerArgs
                    .WithHelp(name: "outer args long enough to wrap to a new line")
                    .ZeroOrMore(),
                symbols: outer => outer.AddCommand(
                    "inner-command", "command help",
                    arguments: args => args
                        .WithHelp(name: "inner-args")
                        .ZeroOrOne()))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var usageText = $"{_executableName} outer-command <outer args long enough to wrap to a new line> [command]";

            var expectedLines = new List<string> { "Usage:" };
            var builder = new StringBuilder();
            foreach (var word in usageText.Split())
            {
                var nextLength = 1 + word.Length + builder.Length;

                if (nextLength > SmallMaxWidth)
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

            GetHelpText().Should().Contain(expected);
        }

        #endregion Usage

        #region Arguments

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_no_commands_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            GetHelpText().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_commands_but_no_arguments_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command", "command help")
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);
            GetHelpText().Should().NotContain("Arguments:");

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);
            GetHelpText().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_included_if_there_are_commands_with_arguments_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand(
                    name: "the-command",
                    description: "command help",
                    arguments: args => args
                        .WithHelp(name: "arg command name", description: "test")
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            GetHelpText().Should().Contain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_options_with_no_arguments_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddOption(
                    new[] { "-v", "--verbosity" },
                    "Sets the verbosity.")
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            GetHelpText().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_is_not_included_if_there_are_only_options_with_arguments_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddOption(
                    new[] { "-v", "--verbosity" },
                    "Sets the verbosity.",
                    arguments: args => args
                        .WithHelp(name: "argument for options")
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .WriteHelp(_console);

            GetHelpText().Should().NotContain("Arguments:");
        }

        [Fact]
        public void Arguments_section_includes_configured_argument_aliases()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "command help",
                cmd => cmd.AddOption(
                    new[] { "-v", "--verbosity" },
                    "Sets the verbosity.",
                    arguments: args => args
                        .WithHelp(name: "LEVEL")
                        .ExactlyOne()))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain("-v, --verbosity <LEVEL>");
            help.Should().Contain("Sets the verbosity.");
        }

        [Fact]
        public void Arguments_section_uses_HelpDefinition_description_if_provided()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "Help text from description",
                arguments: args => args
                    .WithHelp(name: "the-arg", description: "Help text from HelpDefinition")
                    .ExactlyOne())
            .BuildCommandDefinition();

            var expected =
            $"Arguments:{NewLine}" +
            $"{_indentation}<the-arg>{_columnPadding}Help text from HelpDefinition";

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_does_not_contain_argument_with_HelpDefinition_that_IsHidden()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp("test name", "test desc", true)
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var help = GetHelpText();
            help.Should().NotContain("test name");
            help.Should().NotContain("test desc");

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);
            help = GetHelpText();
            help.Should().NotContain("test name");
            help.Should().NotContain("test desc");
        }

        [Fact]
        public void Arguments_section_aligns_arguments_on_new_lines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand(
                    "outer", "HelpDefinition text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: "The argument for the outer command")
                        .ExactlyOne(),
                    symbols: outer => outer
                        .AddCommand(
                            "inner", "HelpDefinition text for the inner command",
                            arguments: innerArgs => innerArgs
                                .WithHelp(
                                    name: "the-inner-command-arg",
                                    description: "The argument for the inner command")
                                .ExactlyOne()))
                .BuildCommandDefinition();

            var expected =
                $"Arguments:{NewLine}" +
                $"{_indentation}<outer-command-arg>    {_columnPadding}The argument for the outer command{NewLine}" +
                $"{_indentation}<the-inner-command-arg>{_columnPadding}The argument for the inner command";

            commandLineBuilder
                .Subcommand("outer")
                .Subcommand("inner")
                .WriteHelp(_console);

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_removes_extra_whitespace()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: "Argument\tfor the   inner command")
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}Argument for the inner command{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_removes_added_newlines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: $"The argument{NewLine}for the{NewLine}inner command")
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}The argument for the inner command{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Arguments_section_properly_wraps_description()
        {
            var longCmdText =
            $"Argument{NewLine}" +
            $"for inner command with line breaks that is long enough to wrap to a" +
            $"{NewLine}new line";

            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = GetHelpBuilder(SmallMaxWidth),
            }
            .AddCommand("outer", "Help text for the outer command",
                arguments: args => args
                    .WithHelp(
                        name: "outer-command-arg",
                        description: longCmdText)
                    .ExactlyOne())
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}Argument for inner command with line breaks{NewLine}" +
            $"{_indentation}                   {_columnPadding}that is long enough to wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        #endregion Arguments

        #region Options

        [Fact]
        public void Options_section_is_not_included_if_no_options_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "description for outer")
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            GetHelpText().Should().NotContain("Options:");
        }

        [Fact]
        public void Options_section_is_not_included_if_only_subcommands_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "description for outer",
                    outer => outer.AddCommand(
                        "inner", "description for inner"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            GetHelpText().Should().NotContain("Options:");
        }

        [Fact]
        public void Options_section_includes_option_with_empty_description()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command", "Does things.",
                    cmd => cmd
                        .AddOption("-x", "")
                        .AddOption("-n", "Not hidden"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain("-x");
            help.Should().Contain("-n");
        }

        [Fact]
        public void Options_section_does_not_contain_option_with_HelpDefinition_that_IsHidden()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command", "Does things.",
                    cmd => cmd
                        .AddOption("-x", "Is Hidden", opt => opt.WithHelp(isHidden: true))
                        .AddOption("-n", "Not Hidden"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain("-n");
            help.Should().Contain("Not Hidden");
            help.Should().NotContain("-x");
            help.Should().NotContain("Is hidden");
        }

        [Fact]
        public void Options_section_aligns_options_on_new_lines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            "An option with 8 characters")
                        .AddOption(
                            new[] { "-b", "--bbbbbbbbbb" },
                            "An option with 15 characters"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            var lines = help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("-a"));
            var optionB = lines.Last(line => line.Contains("-b"));

            optionA.IndexOf("An option").Should().Be(optionB.IndexOf("An option"));
        }

        [Fact]
        public void Retains_single_dash_on_multi_char_option()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("command", "Help Test",
                    c => c.AddOption(
                        new[] { "-multi", "--alt-option" },
                        "HelpDefinition for option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain("-multi");
            help.Should().NotContain("--multi");
        }

        [Fact]
        public void Options_section_retains_multiple_dashes_on_single_char_option()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("command", "Help Test",
                    c => c.AddOption(
                        new[] { "--m", "--alt-option" },
                        "HelpDefinition for option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("command")
                .WriteHelp(_console);

            GetHelpText().Should().Contain("--m");
        }

        [Fact]
        public void Options_section_removes_extra_whitespace()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            "Help   for      the   option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_removes_added_newlines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            $"Help{NewLine}for {NewLine} the{NewLine}option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Options_section_properly_wraps_description()
        {
            var longOptionText =
            $"The option{NewLine}" +
            $"with line breaks that is long enough to wrap to a{NewLine}" +
            $"new line";

            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = GetHelpBuilder(SmallMaxWidth),
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            longOptionText))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}The option with line breaks that is long enough to{NewLine}" +
            $"{_indentation}         {_columnPadding}wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        #endregion Options

        #region Subcommands

        [Fact]
        public void Subcommand_help_does_not_include_names_of_sibling_commands()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand(
                "outer", "outer description",
                outer => {
                    outer.AddCommand(
                        "sibling", "sibling description");
                    outer.AddCommand(
                        "inner", "inner description",
                        inner => inner.AddCommand(
                            "inner-er", "inner-er description",
                            innerEr => innerEr.AddOption(
                                "some-option",
                                "some-option description")));
            })
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .Subcommand("inner")
                .WriteHelp(_console);

            GetHelpText().Should().NotContain("sibling");
        }

        [Fact]
        public void Subcommands_remove_extra_whitespace()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", "inner    command\t help  with whitespace",
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            "Inner    option \twith whitespace")))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var expected =
            $"Commands:{NewLine}" +
            $"{_indentation}inner-command <inner-args>{_columnPadding}inner command help with whitespace{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_remove_added_newlines()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", $"inner{NewLine}command help {NewLine} with {NewLine}newlines",
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            $"Inner {NewLine} command {NewLine}option with{NewLine} newlines")))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var expected =
            $"Commands:{NewLine}" +
            $"{_indentation}inner-command <inner-args>{_columnPadding}inner command help with newlines{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        [Fact]
        public void Subcommands_properly_wraps_description()
        {
            var longSubcommandText =
            $"The{NewLine}" +
            $"subcommand with line breaks that is long enough to wrap to a{NewLine}" +
            $"new line";

            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = GetHelpBuilder(SmallMaxWidth),
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", longSubcommandText,
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" })))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .WriteHelp(_console);

            var expected =
            $"Commands:{NewLine}" +
            $"{_indentation}inner-command <inner-args>{_columnPadding}The subcommand with line breaks that{NewLine}" +
            $"{_indentation}                          {_columnPadding}is long enough to wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Contain(expected);
        }

        #endregion Subcommands
    }
}
