// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;
using static System.Environment;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class HelpViewTests
    {
        [Fact]
        public void Help_can_be_displayed_for_a_specific_invalid_command()
        {
            var parser = new Parser(
                Command("the-command",
                        "Does the thing.",
                        ExactlyOneArgument));

            var result = parser.Parse("the-command");

            result.Execute()
                  .ToString()
                  .Should()
                  .Be($"Required argument missing for command: the-command{NewLine}{parser.HelpView()}");
        }

        [Fact]
        public void Help_can_be_displayed_for_a_specific_invalid_option()
        {
            var parser = new Parser(
                Command("the-command",
                        "Does the thing.",
                        Option("-x", "Specifies value x", ExactlyOneArgument)));

            var result = parser.Parse("the-command -x");

            result.Execute()
                  .ToString()
                  .Should()
                  .Be($"Required argument missing for option: -x{NewLine}{parser.HelpView()}");
        }

        [Fact]
        public void Command_help_view_includes_names_of_parent_commands()
        {
            var command = Command("outer", "",
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("some-option", ""))));

            command["inner"]["inner-er"]
                .HelpView()
                .Should()
                .StartWith("usage: outer inner inner-er [<options>]");
        }

        [Fact]
        public void Command_help_view_does_not_include_names_of_sibling_commands()
        {
            var command = Command("outer", "",
                                  Command("sibling", ""),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("some-option", ""))));

            command["inner"]
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
                        ZeroOrMoreArguments,
                        Option("-x", "Specifies value x", ExactlyOneArgument),
                        Option("-y", "Specifies value y", NoArguments)));

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
    }
}