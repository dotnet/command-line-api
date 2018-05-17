// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Create;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly CommandParser parser;
        private readonly ITestOutputHelper output;

        public CommandTests(ITestOutputHelper output)
        {
            this.output = output;

            var builder = new ArgumentDefinitionBuilder();

            parser = new CommandParser(
                Command("outer", "",
                        Command("inner", "",
                                new OptionDefinition(
                                    "--option",
                                    "",
                                    argumentDefinition: builder.ExactlyOne()))));
        }

        [Fact]
        public void Outer_command_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result.SpecifiedCommand().Parent;

            outer
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result
                .Symbols
                .ElementAt(0);
            var inner = outer
                .Children
                .ElementAt(0);

            inner
                .Name
                .Should()
                .Be("inner");
        }

        [Fact]
        public void Inner_command_option_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result
                .Symbols
                .ElementAt(0);
            var inner = outer
                .Children
                .ElementAt(0);
            var option = inner
                .Children
                .ElementAt(0);

            option
                .Name
                .Should()
                .Be("option");
        }

        [Fact]
        public void Inner_command_option_argument_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result
                .Symbols
                .ElementAt(0);
            var inner = outer
                .Children
                .ElementAt(0);
            var option = inner
                .Children
                .ElementAt(0);

            option
                .Arguments
                .Should()
                .BeEquivalentTo("argument1");
        }

        [Fact]
        public void Commands_at_multiple_levels_can_have_their_own_arguments()
        {
            var parser = new CommandParser(
                Command("outer", "", new ArgumentDefinitionBuilder().ExactlyOne(),
                        Command("inner", "",
                            new ArgumentDefinitionBuilder().ZeroOrMore())));

            var result = parser.Parse("outer arg1 inner arg2 arg3");

            result.SpecifiedCommand()
                  .Parent
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");

            result.SpecifiedCommand()
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }

        [Fact]
        public void ParseResult_Command_identifies_innermost_command()
        {
            var command = Command("outer", "",
                                  Command("sibling", "", new ArgumentDefinitionBuilder().ZeroOrMore()),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  new OptionDefinition(
                                                      "-x",
                                                      "",
                                                      argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()))));

            var result = command.Parse("outer inner inner-er -x arg");

            result.SpecifiedCommand().Name.Should().Be("inner-er");

            result = command.Parse("outer inner");

            result.SpecifiedCommand().Name.Should().Be("inner");
        }

        [Fact]
        public void By_default_the_name_of_the_command_is_the_name_of_the_executable()
        {
            var command = new CommandDefinition(
                new[]
                {
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: null),
                    new OptionDefinition(
                        "-y",
                        "",
                        argumentDefinition: null)
                });

            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void ParseResult_SpecifiedCommandDefinition_identifies_implicit_root_command()
        {
            var parser = new OptionParser(
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: null),
                new OptionDefinition(
                    "-y",
                    "",
                    argumentDefinition: null));

            var result = parser.Parse("-x -y");

            var command = result.SpecifiedCommandDefinition();

            command.Should().NotBeNull();
            command.Name.Should().Be(RootCommand().Name);
        }

        [Fact]
        public void ParsedCommand_identifies_the_ParsedCommand_for_the_innermost_command()
        {
            var command = Command("outer", "",
                                  Command("sibling", "",
                                      new ArgumentDefinitionBuilder().ExactlyOne()),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  new OptionDefinition(
                                                      "-x",
                                                      "",
                                                      argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()))));

            var result = command.Parse("outer inner inner-er -x arg");

            output.WriteLine(result.ToString());

            var parsedOption = result.SpecifiedCommand()["x"];

            parsedOption.GetValueOrDefault().Should().Be("arg");

            result = command.Parse("outer sibling arg");

            result.SpecifiedCommand().GetValueOrDefault().Should().Be("arg");
        }
    }
}
