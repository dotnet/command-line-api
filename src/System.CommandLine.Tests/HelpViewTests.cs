// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;
using static System.CommandLine.Create;

namespace System.CommandLine.Tests
{
    public class HelpViewTests
    {
        private readonly ITestOutputHelper output;

        public HelpViewTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
        {
            var command = Command("outer", "the outer command",
                                  Command("inner", "the inner command",
                                          Command("inner-er", "the inner-er command",
                                                  new OptionDefinition(
                                                      "some-option",
                                                      "some option",
                                                      argumentDefinition: null))));

           command.Subcommand("inner")
                  .Subcommand("inner-er")
                  .HelpView()
                  .Should()
                  .StartWith("Usage: outer inner inner-er [options]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
        {
            var command = Command("outer", "outer description",
                                  Command("sibling", "sibling description"),
                                  Command("inner", "inner description",
                                          Command("inner-er", "inner-er description",
                                                  new OptionDefinition(
                                                      "some-option",
                                                      "some-option description",
                                                      argumentDefinition: null))));

            command
                .Subcommand("inner")
                .HelpView()
                .Should()
                .NotContain("sibling");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_child_commands_under_options_section()
        {
            var command = Command("outer", "description for outer",
                                  Command("inner", "description for inner"));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

           helpView
                .Should()
                .NotContain("Options:");
        }

        [Fact]
        public void An_option_can_be_hidden_from_help_output_by_leaving_its_help_text_empty()
        {
            var command = Command("the-command", "Does things.",
                                  new OptionDefinition(
                                      "-x",
                                      "",
                                      argumentDefinition: null),
                                  new OptionDefinition(
                                      "-n",
                                      "Not hidden",
                                      argumentDefinition: null));

            var help = command.HelpView();

            help.Should().NotContain("-x");
        }

        [Fact]
        public void When_a_command_accepts_arguments_then_the_synopsis_shows_them()
        {
            var command = Command("the-command", "command help",
                Define.Arguments()
                      .WithHelp(name: "the-args")
                      .ZeroOrMore(),
                                  new OptionDefinition(
                                      new[] {"-v", "--verbosity"},
                                      "Sets the verbosity",
                                      argumentDefinition: null));

            var helpView = command.HelpView();

            helpView
                .Should()
                .StartWith("Usage: the-command [options] <the-args>");
        }

        [Fact]
        public void When_a_command_and_subcommand_both_accept_arguments_then_the_synopsis_for_the_inner_command_shows_them()
        {
            var command = Command("outer-command", "command help",
                                   Define.Arguments()
                                       .WithHelp(name: "outer-args")
                                       .ZeroOrMore(),
                                  Command("inner-command", "command help",
                                      Define.Arguments().WithHelp(name: "inner-args")
                                          .ZeroOrOne(),
                                          new OptionDefinition(
                                              "-v|--verbosity",
                                              "Sets the verbosity",
                                              argumentDefinition: null)));

            var helpView = command.Subcommand("inner-command").HelpView();

            output.WriteLine(helpView);

            helpView
                .Should()
                .StartWith("Usage: outer-command <outer-args> inner-command [options] <inner-args>");
        }

        [Fact]
        public void When_a_command_does_not_accept_arguments_then_the_synopsis_does_not_show_them()
        {
            var builder = new ArgumentDefinitionBuilder();
            var command = Command("the-command",
                                  "command help",
                                  ArgumentDefinition.None,
                                  new OptionDefinition(
                                      new[] { "-v", "--verbosity" },
                                      "Sets the verbosity",
                                      argumentDefinition: null));

            var helpView = command.HelpView();

            helpView
                .Should()
                .NotContain("arguments");
        }

        [Fact]
        public void Help_view_wraps_with_aligned_column_when_help_text_contains_newline()
        {
            var builder = new ArgumentDefinitionBuilder();
            var command = Command("the-command",
                                  "command help",
                                  new OptionDefinition(
                                      new[] { "-v", "--verbosity" },
                                      $"Sets the verbosity. Accepted values are:{NewLine}- quiet{NewLine}- loud{NewLine}- very-loud",
                                      argumentDefinition: builder.ExactlyOne()));

            var helpView = command.HelpView();

            var indent = "                    ";

            helpView.Should()
                    .Contain($"Sets the verbosity. Accepted values are:{NewLine}{indent}- quiet{NewLine}{indent}- loud{NewLine}{indent}- very-loud");
        }

        [Fact]
        public void Argument_names_are_included_in_help_view()
        {
            var command = Command("the-command",
                                  "command help",
                                  new OptionDefinition(
                                      new[] { "-v", "--verbosity" },
                                      "Sets the verbosity.",
                                      argumentDefinition: Define.Arguments().WithHelp(name: "LEVEL")
                                                                .ExactlyOne()));

            command.HelpView()
                   .Should()
                   .Contain("  -v, --verbosity <LEVEL>   Sets the verbosity.");
        }

        [Fact]
        public void If_arguments_have_descriptions_then_there_is_an_arguments_section()
        {
            var command = Command("the-command", "The help text for the command",
                Define.Arguments().WithHelp(name: "the-arg",
                        description: "This is the argument for the command.")
                    .ZeroOrOne(),
                new OptionDefinition(
                    new[] { "-o", "--one" },
                    "The first option",
                    argumentDefinition: null));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

            helpView.Should()
                    .Contain($"Arguments:{NewLine}  <the-arg>   This is the argument for the command.");
        }

        [Fact]
        public void Column_for_argument_descriptions_are_vertically_aligned()
        {
            var command = Command("outer", "Help text for the outer command",
                Define.Arguments().WithHelp(name: "outer-command-arg",
                                description: "The argument for the inner command")
                                .ExactlyOne(),
                                  Command("inner", "Help text for the inner command",
                                      Define.Arguments().WithHelp(name: "the-inner-command-arg",
                                          description: "The argument for the inner command")
                                          .ExactlyOne()));

            var helpView =  command.Subcommand("inner").HelpView();

            output.WriteLine(helpView);

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
            var command = Command("the-command", "Help text for the command",
                                  new OptionDefinition(
                                      new[] {"-a", "--aaa"},
                                      "An option with 8 characters",
                                      argumentDefinition: null),
                                  new OptionDefinition(
                                      new[] {"-b", "--bbbbbbbbbb"},
                                      "An option with 15 characters",
                                      argumentDefinition: null));

            var helpView = command.HelpView();

            var lines = helpView.Split(new[]{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Last(line => line.Contains("-a"));
            var optionB = lines.Last(line => line.Contains("-b"));

            optionA.IndexOf("An option")
                   .Should()
                   .Be(optionB.IndexOf("An option"));
        }

        [Fact]
        public void When_unmatched_tokens_are_allowed_then_help_view_indicates_it()
        {
            var command = Command("some-command", "Does something",
                                  treatUnmatchedTokensAsErrors: false,
                                  symbolDefinitions: new OptionDefinition(
                                      "-x",
                                      "Indicates whether x",
                                      argumentDefinition: null));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

            helpView.Should().StartWith("Usage: some-command [options] [[--] <additional arguments>...]]");
        }

        [Fact]
        public void Retain_single_dash_on_multi_char_option()
        {
            var command = Command("command", "Help Test",
                new OptionDefinition(
                    new[] {"-multi", "--alt-option"},
                    "Help for option",
                    argumentDefinition: null));
            var helpView = command.HelpView();
            helpView.Should().Contain("-multi");
            helpView.Should().NotContain("--multi");
        }

        [Fact]
        public void Retain_multiple_dashes_on_single_char_option()
        {
            var command = Command("command", "Help Test",
                new OptionDefinition(
                    new[] {"--m", "--alt-option"},
                    "Help for option",
                    argumentDefinition: null));
            var helpView = command.HelpView();
            helpView.Should().Contain("--m");
        }
    }
}
