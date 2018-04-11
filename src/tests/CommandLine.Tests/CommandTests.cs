// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly CommandParser parser;

        public CommandTests()
        {
            parser = new CommandParser(
                Command("outer", "",
                        Command("inner", "",
                                Option("--option", "",
                                       arguments: ExactlyOneArgument()))));
        }

        [Fact(Skip = "Redesign access to parent commands from parse result")]
        public void Outer_command_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result.ParsedCommand()["outer"];

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
                .ParsedOptions
                .ElementAt(0);
            var inner = outer
                .ParsedOptions
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
                .ParsedOptions
                .ElementAt(0);
            var inner = outer
                .ParsedOptions
                .ElementAt(0);
            var option = inner
                .ParsedOptions
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
                .ParsedOptions
                .ElementAt(0);
            var inner = outer
                .ParsedOptions
                .ElementAt(0);
            var option = inner
                .ParsedOptions
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
                Command("outer", "",
                        ExactlyOneArgument(),
                        Command("inner", "",
                                ZeroOrMoreArguments())));

            var result = parser.Parse("outer arg1 inner arg2 arg3");

                // FIX: (Commands_at_multiple_levels_can_have_their_own_arguments)   result["outer"]
//                .Arguments
//                .Should()
//                .BeEquivalentTo("arg1");

            result.ParsedCommand()
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }

        [Fact]
        public void ParseResult_Command_identifies_innermost_command()
        {
            var command = Command("outer", "",
                                  Command("sibling", "", ZeroOrMoreArguments()),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("-x", "", ZeroOrMoreArguments()))));

            var result = command.Parse("outer inner inner-er -x arg");

            result.Command().Name.Should().Be("inner-er");

            result = command.Parse("outer inner");

            result.Command().Name.Should().Be("inner");
        }

        [Fact]
        public void By_default_the_name_of_the_command_is_the_name_of_the_executable()
        {
            var command = new Command(
                new[]
                {
                    Option("-x", ""),
                    Option("-y", "")
                });

            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void ParseResult_Command_identifies_implicit_root_command()
        {
            var parser1 = new OptionParser(
                Option("-x", ""),
                Option("-y", ""));

            var result = parser1.Parse("-x -y");

            var command = result.Command();

            command.Should().NotBeNull();
            command.Name.Should().Be(RootCommand().Name);
        }

        [Fact]
        public void ParsedCommand_identifies_the_ParsedCommand_for_the_innermost_command()
        {
            var command = Command("outer", "",
                                  Command("sibling", "",
                                          ExactlyOneArgument()),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("-x", "",
                                                         ExactlyOneArgument()))));

            var result = command.Parse("outer inner inner-er -x arg");

            var parsedOption = result.ParsedCommand()["x"];

            parsedOption.Value().Should().Be("arg");

            result = command.Parse("outer sibling arg");

            result.ParsedCommand().Value().Should().Be("arg");
        }

        [Fact]
        public void Option_Command_identifies_the_parent_Command()
        {
            var option = Option("option", "");
            var inner = Command("inner", "", option);

            option.Command().Should().Be(inner);
        }

        [Fact]
        public void Command_Command_identifies_self()
        {
            var inner = Command("inner", "");
            var outer = Command("outer", "", inner);

            outer["inner"].Command().Should().Be(inner);
        }

        [Fact]
        public void Subcommands_names_are_available_as_suggestions()
        {
            var command = Command("test", "",
                ExactlyOneArgument(),
                new Command("foo", "Command one"),
                new Command("bar", "Command two"));

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("foo", "bar");
        }
    }
}