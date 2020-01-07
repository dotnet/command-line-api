// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ParseDiagramTests
    {
        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            var parser = new Parser(
                new Command(
                    "the-command")
                  {
                        new Option("-x")
                        {
                            Argument = new Argument
                            {
                                Arity = ArgumentArity.ExactlyOne
                            }
                        },
                        new Option("-y"),
                        new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    });

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_displays_unmatched_tokens()
        {
            var command = new Command("command")
            {
                new Option<string>("-x").FromAmong("arg1", "arg2", "arg3")
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
                new Option("-f")
                {
                    Argument = new Argument<int>()
                }
            };

            var result = command.Parse("-f not-an-int");

            result.Diagram()
                  .Should()
                  .Be($"[ {RootCommand.ExecutableName} [ -f !<not-an-int> ] ]");
        }

        [Fact]
        public void Parse_diagram_identifies_options_where_default_values_have_been_applied()
        {
            var rootCommand = new RootCommand
            {
                new Option(new[] { "-h", "--height" })
                {
                    Argument = new Argument<int>(getDefaultValue: () => 10), Description = ""
                },
                new Option(new[] { "-w", "--width" })
                {
                    Argument = new Argument<int>(getDefaultValue: () => 15), Description = ""
                },
                new Option(new[] { "-c", "--color" })
                {
                    Argument = new Argument<ConsoleColor>(() => ConsoleColor.Cyan), Description = ""
                }
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
                new Argument<string> { Name = "first" },
                new Argument<string> { Name = "second" },
                new Argument<string[]> { Name = "third" }
            };

            var result = command.Parse("one two three four five");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ first <one> ] [ second <two> ] [ third <three> <four> <five> ] ]");
        }
    }
}
