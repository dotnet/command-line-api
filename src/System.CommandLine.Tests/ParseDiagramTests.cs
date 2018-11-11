// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
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
                    "the-command", "",
                    new[] {
                        new Option(
                            "-x",
                            "Specifies value x",
                            new Argument
                            {
                                Arity = ArgumentArity.ExactlyOne
                            }),
                        new Option(
                            "-y",
                            "Specifies value y")
                    },
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }));

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_displays_unmatched_tokens()
        {
            var parser = new Parser(
                new Command("command", "",
                            new[]
                            {
                                new Option(
                                    "-x",
                                    "",
                                    new Argument
                                    {
                                        Arity = ArgumentArity.ExactlyOne
                                    }
                                    .FromAmong("arg1", "arg2", "arg3"))
                            }));

            var result = parser.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command ![ -x ] ]   ???--> ar");
        }

        [Fact]
        public void Parse_diagram_shows_type_conversion_errors()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-f", "",
                                    args => args.ParseArgumentsAs<int>())
                         .Build();

            var result = parser.Parse("-f not-an-int");

            result.Diagram()
                  .Should()
                  .Be($"[ {RootCommand.ExeName} ![ -f <not-an-int> ] ]");
        }

        [Fact]
        public void Parse_diagram_identifies_implicitly_applied_options()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(new[] { "-h", "--height" }, "",
                                    args => args.WithDefaultValue(() => 10)
                                                .ExactlyOne())
                         .AddOption(new[] { "-w", "--width" }, "",
                                    args => args.WithDefaultValue(() => 15)
                                                .ExactlyOne())
                         .AddOption(new[] { "-c", "--color" }, "",
                                    args => args.WithDefaultValue(() => ConsoleColor.Cyan)
                                                .ExactlyOne())
                         .Build();

            var result = parser.Parse("-w 9000");

            var diagram = result.Diagram();

            diagram.Should()
                   .Be($"[ {RootCommand.ExeName} [ -w <9000> ] *[ --height <10> ] *[ --color <Cyan> ] ]");
        }
    }
}
