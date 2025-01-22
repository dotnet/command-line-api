// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ParseDiagramTests
    {
        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            Command command = 
                new Command(
                    "the-command")
                {
                    new Option<string>("-x"),
                    new Option<bool>("-y"),
                    new Argument<string[]>("arg")
                };

            var result = command.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_displays_unmatched_tokens()
        {
            Option<string> option = new ("-x");
            option.AcceptOnlyFromAmong("arg1", "arg2", "arg3");

            var command = new Command("command")
            {
                option
            };

            var result = command.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command ![ -x <ar> ] ]");
        }

        [Fact]
        public void Parse_diagram_shows_type_conversion_errors()
        {
            var command = new RootCommand
            {
                new Option<int>("-f")
            };

            var result = command.Parse("-f not-an-int");

            result.Diagram()
                  .Should()
                  .Be($"[ {RootCommand.ExecutableName} ![ -f <not-an-int> ] ]");
        }

        [Fact]
        public void Parse_diagram_identifies_options_where_default_values_have_been_applied()
        {
            var rootCommand = new RootCommand
            {
                new Option<int>("--height", "-h") { DefaultValueFactory = _ => 10 },
                new Option<int>("-w", "--width") { DefaultValueFactory = _ => 15 },
                new Option<ConsoleColor>("--color", "-c") { DefaultValueFactory = _ => ConsoleColor.Cyan }
            };

            var result = rootCommand.Parse("-w 9000");

            var diagram = result.Diagram();

            diagram.Should()
                   .Be($"[ {RootCommand.ExecutableName} [ -w <9000> ] *[ --height <10> ] *[ --color <Cyan> ] ]");
        }

        [Fact]
        public void Parse_diagram_indicates_which_tokens_were_applied_to_which_command_argument()
        {
            var command = new Command("the-command")
            {
                new Argument<string>("first"),
                new Argument<string> ("second"),
                new Argument<string[]> ("third")
            };

            var result = command.Parse("one two three four five");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ first <one> ] [ second <two> ] [ third <three> <four> <five> ] ]");
        }
        
        [Fact]
        public void Parse_diagram_indicates_which_tokens_were_applied_to_which_command_argument_for_sequences_of_complex_types()
        {
            var command = new Command("the-command")
            {
                new Argument<FileInfo> ("first"),
                new Argument<FileInfo> ("second"),
                new Argument<FileInfo[]> ("third")
            };

            var result = command.Parse("one two three four five");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ first <one> ] [ second <two> ] [ third <three> <four> <five> ] ]");
        }
    }
}
