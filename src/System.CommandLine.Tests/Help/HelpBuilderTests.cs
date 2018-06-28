// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public class HelpBuilderTests
    {
        private const int MaxWidth = 70;
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
            _helpBuilder = new HelpBuilder(
                console: _console,
                columnGutter: ColumnGutterWidth,
                indentationSize: IndentationWidth,
                maxWidth: MaxWidth
            );

            _output = output;
            _columnPadding = new string(' ', ColumnGutterWidth);
            _indentation = new string(' ', IndentationWidth);
            _executableName = CommandLineBuilder.ExeName;
        }

        private string GetHelpText()
        {
            return _console.Out.ToString();
        }

        #region Synopsis

        [Fact]
        public void When_a_command_accepts_arguments_then_the_synopsis_shows_them()
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

            var help = GetHelpText();
            help.Should().Contain($"Usage:{NewLine}{_indentation}{_executableName} the-command [options] <the-args>");
        }

        [Fact]
        public void When_a_command_and_subcommand_both_accept_arguments_then_the_synopsis_for_the_inner_command_shows_them()
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

            var help = GetHelpText();
            help.Should().Contain($"Usage:{NewLine}{_indentation}{_executableName} outer-command <outer-args> inner-command [options]{NewLine}{_indentation}<inner-args>");
        }

        [Fact]
        public void When_a_command_does_not_accept_arguments_then_the_synopsis_does_not_show_them()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "command help",
                      cmd => cmd.AddOption(
                          new[] { "-v", "--verbosity" },
                          "Sets the verbosity"))
            .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var help = GetHelpText();
            help.Should().NotContain("arguments");
        }

        [Fact]
        public void When_unmatched_tokens_are_allowed_then_help_view_indicates_it()
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

            var help = GetHelpText();
            help.Should().Contain($"Usage:{NewLine}{_indentation}{_executableName} some-command [options] [[--] <additional arguments>...]]");
        }

        [Fact]
        public void Extra_whitespace_is_removed_in_synopsis()
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
            $"{_indentation}test description for synopsis{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_removed_in_synopsis()
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
            $"{_indentation}test description with line breaks{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Properly_wraps_description_in_synopsis()
        {
            var longText = $"test{NewLine}description with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = longText,
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description with line breaks that is long enough to wrap to a{NewLine}" +
            $"{_indentation}new line{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Synopsis

        #region Usage

        [Fact]
        public void Extra_whitespace_is_removed_in_usage()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", "Help text   for the outer   command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help text for the outer command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_removed_in_usage()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", $"Help text{NewLine}for the outer{NewLine}command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help text for the outer command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Properly_wraps_description_in_usage()
        {
            var longText = $"Help{NewLine}text with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", longText,
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $@"{_executableName}:{NewLine}" +
            $"{_indentation}test description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help text with line breaks that is long enough to wrap to{NewLine}" +
            $"{_indentation}     {_columnPadding}a new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Usage

        #region Arguments

        [Fact]
        public void Argument_section_is_not_included_if_no_arguments()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "command help")
            .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var help = GetHelpText();
            help.Should().NotContain("Arguments");
        }

        [Fact]
        public void Argument_aliases_are_included_in_help_view()
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
            help.Should().Contain($"{_indentation}-v, --verbosity <LEVEL>{_columnPadding}Sets the verbosity.");
        }

        [Fact]
        public void If_arguments_have_descriptions_then_there_is_an_arguments_section()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
            .AddCommand("the-command", "The help text for the command",
                      arguments: args => args.WithHelp(name: "the-arg",
                                                       description: "This is the argument for the command.")
                                             .ZeroOrOne(),
                      symbols: cmd => cmd.AddOption(
                          new[] { "-o", "--one" },
                          "The first option"))
            .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain($"Arguments:{NewLine}{_indentation}<the-arg>{_columnPadding}This is the argument for the command.");
        }

        [Fact]
        public void An_argument_is_included_in_help_if_no_HelpDefinition_is_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain($"Arguments:{NewLine}  <>");
        }


        [Fact]
        public void An_argument_is_included_in_help_if_HelpDefinition_is_configured()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _helpBuilder,
            }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp("test name", "test desc")
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain($"Arguments:{NewLine}{_indentation}<test name>{_columnPadding}test desc");
        }

        [Fact]
        public void An_argument_does_not_produce_help_if_help_is_hidden()
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
            help.Should().NotContain("test desc");
        }

        [Fact]
        public void Column_for_argument_descriptions_are_vertically_aligned()
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
                            description: "The argument for the inner command")
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

            commandLineBuilder
                .Subcommand("outer")
                .Subcommand("inner")
                .WriteHelp(_console);

            var help = GetHelpText();
            var lines = help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("outer-command-arg"));
            var optionB = lines.Last(line => line.Contains("the-inner-command-arg"));

            optionA.IndexOf("The argument").Should().Be(optionB.IndexOf("The argument"));
        }

        [Fact]
        public void Extra_whitespace_is_removed_in_arguments()
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
            $@"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}Argument for the inner command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_removed_in_arguments()
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
            $@"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}The argument for the inner command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Properly_wraps_description_in_arguments()
        {
            var longText = $"Argument{NewLine}for inner command with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: longText)
                        .ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $@"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}Argument for inner command with line breaks{NewLine}" +
            $"{_indentation}                   {_columnPadding}that is long enough to wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Arguments

        #region Options

        [Fact]
        public void An_option_is_included_in_help_output_if_its_description_is_empty()
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
        public void An_option_is_hidden_from_help_output_if_it_is_flagged_as_hidden()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command", "Does things.",
                    cmd => cmd
                        .AddOption("-x", "Is Hidden", opt => opt.WithHelp(isHidden: true))
                        .AddOption("-n", "Not hidden"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().Contain("-n");
            help.Should().NotContain("-x");
        }

        [Fact]
        public void Column_for_option_descriptions_are_vertically_aligned()
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
        public void Retains_multiple_dashes_on_single_char_option()
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

            var help = GetHelpText();
            help.Should().Contain("--m");
        }

        [Fact]
        public void Help_view_wraps_with_aligned_column_when_help_text_contains_newline()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("the-command",
                    "command help",
                    cmd => cmd
                        .AddOption(
                            new[] { "-v", "--verbosity" },
                            "Sets the verbosity. Accepted values are: [quiet] [loud] [very-loud]",
                            arguments: args => args.ExactlyOne()))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("the-command")
                .WriteHelp(_console);

            const string indent = "                     ";

            var help = GetHelpText();
            help.Should().Contain($"Sets the verbosity. Accepted values are: [quiet]{NewLine}{indent}[loud] [very-loud]");
        }

        [Fact]
        public void Extra_whitespace_is_removed_in_options()
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
            $@"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_removed_in_options()
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
            $@"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help for the option{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Properly_wraps_description_in_options()
        {
            var longText = $"The option{NewLine}with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            longText))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $@"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}The option with line breaks that is long enough to{NewLine}" +
            $"{_indentation}         {_columnPadding}wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Options

        #region Subcommands

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
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

            var help = GetHelpText();
            help.Should().Contain($"Usage:{NewLine}{_indentation}{CommandLineBuilder.ExeName} outer inner inner-er [options]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
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

            var help = GetHelpText();
            help.Should().NotContain("sibling");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_subcommands_under_options_section()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _helpBuilder,
                }
                .AddCommand("outer", "description for outer",
                    outer => outer.AddCommand("inner", "description for inner"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var help = GetHelpText();
            help.Should().NotContain("Options:");
        }

        [Fact]
        public void Extra_whitespace_is_removed_in_subcommands()
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
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $@"inner-command:{NewLine}" +
            $"{_indentation}inner command help with whitespace{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command [options]{NewLine}" +
            $"{_indentation}<inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}Inner option with whitespace{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_removed_in_subcommands()
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
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $@"inner-command:{NewLine}" +
            $"{_indentation}inner command help with newlines{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command [options]{NewLine}" +
            $"{_indentation}<inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}Inner command option with newlines{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Properly_wraps_description_in_subcommands()
        {
            var longText = $"The{NewLine}subcommand with line breaks that is long enough to wrap to a{NewLine}new line";
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
                        "inner-command", longText,
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            longText)))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $@"inner-command:{NewLine}" +
            $"{_indentation}The subcommand with line breaks that is long enough to wrap to a{NewLine}" +
            $"{_indentation}new line{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command [options]{NewLine}" +
            $"{_indentation}<inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}The subcommand with line breaks that is long{NewLine}" +
            $"{_indentation}               {_columnPadding}enough to wrap to a new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Subcommands
    }
}
