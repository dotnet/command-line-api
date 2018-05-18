// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class HelpViewTests
    {
        private readonly ITestOutputHelper _output;

        public HelpViewTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
        {
            var command = new ParserBuilder()
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

            command.Subcommand("inner")
                   .Subcommand("inner-er")
                   .HelpView()
                   .Should()
                   .StartWith("Usage: outer inner inner-er [options]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
        {
            var command = new ParserBuilder()
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

            command
                .Subcommand("inner")
                .HelpView()
                .Should()
                .NotContain("sibling");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_child_commands_under_options_section()
        {
            var command = new ParserBuilder()
                          .AddCommand("outer", "description for outer",
                                      outer =>
                                          outer.AddCommand("inner", "description for inner"))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            _output.WriteLine(helpView);

            helpView
                .Should()
                .NotContain("Options:");
        }

        [Fact]
        public void An_option_can_be_hidden_from_help_output_by_leaving_its_help_text_empty()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "Does things.",
                                      cmd => cmd.AddOption("-x", "")
                                                .AddOption("-n", "Not hidden"))
                          .BuildCommandDefinition();

            var help = command.HelpView();

            help.Should().NotContain("-x");
        }

        [Fact]
        public void When_a_command_accepts_arguments_then_the_synopsis_shows_them()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "command help",
                                      arguments: args => args
                                                         .WithHelp(name: "the-args")
                                                         .ZeroOrMore(),
                                      symbols: cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity"))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            helpView
                .Should()
                .StartWith("Usage: the-command [options] <the-args>");
        }

        [Fact]
        public void When_a_command_and_subcommand_both_accept_arguments_then_the_synopsis_for_the_inner_command_shows_them()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer-command", "command help",
                              arguments: outerArgs => outerArgs
                                                      .WithHelp(name: "outer-args")
                                                      .ZeroOrMore(),
                              symbols: outer => outer.AddCommand(
                                  "inner-command", "command help",
                                  arguments: args => args.WithHelp(name: "inner-args")
                                                         .ZeroOrOne(),
                                  symbols: inner => inner.AddOption(
                                      "-v|--verbosity",
                                      "Sets the verbosity")))
                          .BuildCommandDefinition();

            var helpView = command.Subcommand("inner-command").HelpView();

            _output.WriteLine(helpView);

            helpView
                .Should()
                .StartWith("Usage: outer-command <outer-args> inner-command [options] <inner-args>");
        }

        [Fact]
        public void When_a_command_does_not_accept_arguments_then_the_synopsis_does_not_show_them()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity"))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            helpView
                .Should()
                .NotContain("arguments");
        }

        [Fact]
        public void Help_view_wraps_with_aligned_column_when_help_text_contains_newline()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command",
                                      "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          $"Sets the verbosity. Accepted values are:{NewLine}- quiet{NewLine}- loud{NewLine}- very-loud",
                                          arguments: args => args.ExactlyOne()))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            var indent = "                    ";

            helpView.Should()
                    .Contain($"Sets the verbosity. Accepted values are:{NewLine}{indent}- quiet{NewLine}{indent}- loud{NewLine}{indent}- very-loud");
        }

        [Fact]
        public void Argument_names_are_included_in_help_view()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command",
                                      "command help",
                                      cmd => cmd.AddOption(
                                          new[] { "-v", "--verbosity" },
                                          "Sets the verbosity.",
                                          arguments: args => args.WithHelp(name: "LEVEL")
                                                                 .ExactlyOne()))
                          .BuildCommandDefinition();

            command.HelpView()
                   .Should()
                   .Contain("  -v, --verbosity <LEVEL>   Sets the verbosity.");
        }

        [Fact]
        public void If_arguments_have_descriptions_then_there_is_an_arguments_section()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "The help text for the command",
                                      arguments: args => args.WithHelp(name: "the-arg",
                                                                       description: "This is the argument for the command.")
                                                             .ZeroOrOne(),
                                      symbols: cmd => cmd.AddOption(
                                          new[] { "-o", "--one" },
                                          "The first option"))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            _output.WriteLine(helpView);

            helpView.Should()
                    .Contain($"Arguments:{NewLine}  <the-arg>   This is the argument for the command.");
        }

        [Fact]
        public void Column_for_argument_descriptions_are_vertically_aligned()
        {
            var command = new ParserBuilder()
                          .AddCommand(
                              "outer", "Help text for the outer command",
                              arguments: args => args.WithHelp(name: "outer-command-arg",
                                                               description: "The argument for the inner command")
                                                     .ExactlyOne(),
                              symbols: outer => outer.AddCommand(
                                  "inner", "Help text for the inner command",
                                  arguments: innerArgs => innerArgs.WithHelp(name: "the-inner-command-arg",
                                                                             description: "The argument for the inner command")
                                                                   .ExactlyOne()))
                          .BuildCommandDefinition();

            var helpView = command.Subcommand("inner").HelpView();

            _output.WriteLine(helpView);

            var lines = helpView.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("outer-command-arg"));
            var optionB = lines.Last(line => line.Contains("the-inner-command-arg"));

            optionA.IndexOf("The argument")
                   .Should()
                   .Be(optionB.IndexOf("The argument"));
        }

        [Fact]
        public void Column_for_options_descriptions_are_vertically_aligned()
        {
            var command = new ParserBuilder()
                          .AddCommand("the-command", "Help text for the command",
                                      symbols =>
                                          symbols.AddOption(
                                                     new[] { "-a", "--aaa" },
                                                     "An option with 8 characters")
                                                 .AddOption(
                                                     new[] { "-b", "--bbbbbbbbbb" },
                                                     "An option with 15 characters"))
                          .BuildCommandDefinition();

            var helpView = command.HelpView();

            var lines = helpView.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("-a"));
            var optionB = lines.Last(line => line.Contains("-b"));

            optionA.IndexOf("An option")
                   .Should()
                   .Be(optionB.IndexOf("An option"));
        }

        [Fact]
        public void When_unmatched_tokens_are_allowed_then_help_view_indicates_it()
        {
            var command =
                new ParserBuilder()
                    .TreatUnmatchedTokensAsErrors(false)
                    .AddCommand("some-command", "Does something",
                                c => c.AddOption(
                                    "-x",
                                    "Indicates whether x"))
                    .BuildCommandDefinition();

            var helpView = command.HelpView();

            _output.WriteLine(helpView);

            helpView.Should().StartWith("Usage: some-command [options] [[--] <additional arguments>...]]");
        }

        [Fact]
        public void Retain_single_dash_on_multi_char_option()
        {
            var command = new ParserBuilder()
                          .AddCommand("command", "Help Test",
                                      c => c.AddOption(
                                          new[] { "-multi", "--alt-option" },
                                          "Help for option"))
                          .BuildCommandDefinition();
            var helpView = command.HelpView();
            helpView.Should().Contain("-multi");
            helpView.Should().NotContain("--multi");
        }

        [Fact]
        public void Retain_multiple_dashes_on_single_char_option()
        {
            var command = new ParserBuilder()
                          .AddCommand("command", "Help Test",
                                      c => c.AddOption(
                                          new[] { "--m", "--alt-option" },
                                          "Help for option"))
                          .BuildCommandDefinition();
            var helpView = command.HelpView();
            helpView.Should().Contain("--m");
        }
    }
}
