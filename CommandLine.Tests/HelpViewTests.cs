// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class HelpViewTests
    {
        private readonly ITestOutputHelper output;

        public HelpViewTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Help_can_be_displayed_for_a_specific_invalid_command()
        {
            var command = Command("the-command",
                                  "Does the thing.",
                                  ExactlyOneArgument());
            var parser = new Parser(command);

            var result = parser.Parse("the-command");

            result.Execute()
                  .ToString()
                  .Should()
                  .Be($"Required argument missing for command: the-command{NewLine}{command.HelpView()}");
        }

        [Fact]
        public void Help_can_be_displayed_for_a_specific_invalid_option()
        {
            var command = Command("the-command",
                                  "Does the thing.",
                                  Option("-x", "Specifies value x", ExactlyOneArgument()));
            var parser = new Parser(command);

            var result = parser.Parse("the-command -x");

            result.Execute()
                  .ToString()
                  .Should()
                  .Be($"Required argument missing for option: -x{NewLine}{command.HelpView()}");
        }

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
        {
            var command = Command("outer", "the outer command",
                                  Command("inner", "the inner command",
                                          Command("inner-er", "the inner-er command",
                                                  Option("some-option", "some option"))));

            ((Command) command["inner"]["inner-er"])
                .HelpView()
                .Should()
                .Contain("Usage: outer inner inner-er [options]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
        {
            var command = Command("outer", "",
                                  Command("sibling", ""),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("some-option", ""))));

            ((Command) command["inner"])
                .HelpView()
                .Should()
                .NotContain("sibling");
        }

        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            var parser = new Parser(
                Command("the-command",
                        "Does the thing.",
                        ZeroOrMoreArguments(),
                        Option("-x", "Specifies value x", ExactlyOneArgument()),
                        Option("-y", "Specifies value y", NoArguments())));

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_helps_explain_partial_parse_operation()
        {
            var parser = new Parser(
                Command("command", "",
                        Option("-x", "",
                               arguments: AnyOneOf("arg1", "arg2", "arg3"))));

            var result = parser.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command [ -x ] ]   ???--> ar");
        }

        [Fact]
        public void An_option_can_be_hidden_from_help_output_by_leaving_its_help_text_empty()
        {
            var command = Command("the-command", "Does things.",
                                  Option("-x", ""),
                                  Option("-n", "Not hidden"));

            var help = command.HelpView();

            help.Should().NotContain("-x");
        }

        [Fact]
        public void An_command_can_be_hidden_from_completions_by_leaving_its_help_empty()
        {
            var command = Command("the-command", "Does things.",
                                  Option("-x", ""),
                                  Option("-n", "Not hidden"));

            var suggestions = command.Parse("the-command ").Suggestions();

            suggestions.Should().NotContain("-x");
        }

        [Fact]
        public void When_a_command_accepts_arguments_then_the_syntax_diagram_shows_them()
        {
            var command = Command("the-command", "command help",
                                  ZeroOrMoreArguments().With(name: "the-args"),
                                  Option("-v|--verbosity", "Sets the verbosity"));

            var helpView = command.HelpView();

            helpView
                .Should()
                .Contain("Usage: the-command [options] <the-args>");
        }

        [Fact]
        public void When_a_command_and_subcommand_both_accept_arguments_then_the_syntax_diagram_for_the_inner_command_shows_them()
        {
            var command = Command("outer-command", "command help",
                                  ZeroOrMoreArguments().With(name: "outer-args"),
                                  Command("inner-command", "command help",
                                          ZeroOrOneArgument().With(name: "inner-args"),
                                          Option("-v|--verbosity", "Sets the verbosity")));

            var helpView = ((Command) command["inner-command"]).HelpView();

            helpView
                .Should()
                .Contain("Usage: outer-command <outer-args> inner-command [options] <inner-args>");
        }

        [Fact]
        public void When_a_command_does_not_accept_arguments_then_the_syntax_diagram_does_not_show_them()
        {
            var command = Command("the-command",
                                  "command help",
                                  NoArguments(),
                                  Option("-v|--verbosity", "Sets the verbosity"));

            var helpView = command.HelpView();

            helpView
                .Should()
                .NotContain("arguments");
        }

        [Fact]
        public void Help_view_wraps_with_aligned_column_when_help_text_contains_newline()
        {
            var command = Command("the-command",
                                  "command help",
                                  Option("-v|--verbosity",
                                         $"Sets the verbosity. Accepted values are:{NewLine}- quiet{NewLine}- loud{NewLine}- very-loud", ExactlyOneArgument()));

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
                                  Option("-v|--verbosity",
                                         "Sets the verbosity.",
                                         ExactlyOneArgument().With(name: "LEVEL")));

            command.HelpView()
                   .Should()
                   .Contain("  -v, --verbosity <LEVEL>   Sets the verbosity.");
        }

        [Fact]
        public void If_arguments_have_descriptions_then_there_is_an_arguments_section()
        {
            var command = Command("the-command", "The help text for the command",
                                  ZeroOrOneArgument()
                                      .With(name: "the-arg",
                                            description: "This is the argument for the command."),
                                  Option("-o|--one", "The first option"));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

            helpView.Should()
                    .Contain($"Arguments:{NewLine}  <the-arg>    This is the argument for the command.");
        }

        [Fact]
        public void Column_for_options_descriptions_are_vertically_aligned()
        {
            var command = Command("the-command", "Help text for the command",
                                  Option("-a|--aaa", "An option with 8 characters"),
                                  Option("-b|--bbbbbbbbbb", "An option with 15 characters"));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

            var lines = helpView.Split(new[]{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var optionA = lines.Single(line => line.Contains("-a"));
            var optionB = lines.Single(line => line.Contains("-b"));

            optionA.IndexOf("An option")
                   .Should()
                   .Be(optionB.IndexOf("An option"));
        }

        [Fact]
        public void When_unmatched_tokens_are_allowed_then_help_view_indicates_it()
        {
            var command = Command("some-command", "Does something",
                                  treatUnmatchedTokensAsErrors: false,
                                  options: Option("-x", "Indicates whether x"));

            var helpView = command.HelpView();

            output.WriteLine(helpView);

            helpView.Should().Contain("Usage: some-command [options] [[--] <additional arguments>...]]");
        }
    }
}