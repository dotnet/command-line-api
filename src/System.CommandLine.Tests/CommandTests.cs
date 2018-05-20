// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly Parser _parser;
        private readonly ITestOutputHelper _output;

        public CommandTests(ITestOutputHelper output)
        {
            _output = output;

            var builder = new ArgumentDefinitionBuilder();

            _parser = new Parser(
                new CommandDefinition("outer", "", new[] {
                    new CommandDefinition("inner", "", new[] {
                        new OptionDefinition(
                            "--option",
                            "",
                            argumentDefinition: builder.ExactlyOne())
                    })
                }));
        }

        [Fact]
        public void Outer_command_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            var outer = result.Command().Parent;

            outer
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

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
            var result = _parser.Parse("outer inner --option argument1");

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
            var result = _parser.Parse("outer inner --option argument1");

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
            var parser = new ParserBuilder()
                         .AddCommand("outer", "",
                                     symbols: outer => outer.AddCommand("inner", "",
                                                                        arguments: innerArgs => innerArgs.ZeroOrMore()),
                                     arguments: outerArgs => outerArgs.ExactlyOne())
                         .Build();

            var result = parser.Parse("outer arg1 inner arg2 arg3");

            result.Command()
                  .Parent
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");

            result.Command()
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }

        [Fact]
        public void ParseResult_Command_identifies_innermost_command()
        {
            var command = new CommandDefinitionBuilder("outer")
                          .AddCommand("inner", "",
                                      symbols: sibling => sibling.AddCommand("inner-er", "",
                                                                             arguments: args => args.ZeroOrMore()))
                          .AddCommand("sibling", "",
                                      arguments: args => args.ZeroOrMore())
                          .BuildCommandDefinition();

            var result = command.Parse("outer inner inner-er -x arg");

            result.Command().Name.Should().Be("inner-er");

            result = command.Parse("outer inner");

            result.Command().Name.Should().Be("inner");
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
        public void When_no_commands_are_added_then_ParseResult_SpecifiedCommandDefinition_identifies_executable()
        {
            var parser = new ParserBuilder()
                         .AddOption("-x", "")
                         .AddOption("-y", "")
                         .Build();

            var result = parser.Parse("-x -y");

            var command = result.CommandDefinition();

            command.Should().NotBeNull();
            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void ParsedCommand_identifies_the_ParsedCommand_for_the_innermost_command()
        {
            var command = new CommandDefinition("outer", "", new[] {
                new CommandDefinition("sibling", "", symbolDefinitions: null, argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()), new CommandDefinition("inner", "", new[] {
                    new CommandDefinition("inner-er", "", new[] {
                        new OptionDefinition(
                            "-x",
                            "",
                            argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())
                    })
                })
            });

            var result = command.Parse("outer inner inner-er -x arg");

            _output.WriteLine(result.ToString());

            var parsedOption = result.Command()["x"];

            parsedOption.GetValueOrDefault().Should().Be("arg");

            result = command.Parse("outer sibling arg");

            result.Command().GetValueOrDefault().Should().Be("arg");
        }
    }
}
