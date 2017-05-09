// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using System.Linq;
using Xunit;
using static System.Console;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly Parser parser;

        public CommandTests()
        {
            parser = new Parser(
                Command("outer", "",
                        Command("inner", "",
                                Option("--option", "",
                                       arguments: ExactlyOneArgument()))));
        }

        [Fact]
        public void Outer_command_is_identified_correctly()
        {
            var result = parser.Parse("outer inner --option argument1");

            var outer = result["outer"];

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
                .AppliedOptions
                .ElementAt(0);
            var inner = outer
                .AppliedOptions
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
                .AppliedOptions
                .ElementAt(0);
            var inner = outer
                .AppliedOptions
                .ElementAt(0);
            var option = inner
                .AppliedOptions
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
                .AppliedOptions
                .ElementAt(0);
            var inner = outer
                .AppliedOptions
                .ElementAt(0);
            var option = inner
                .AppliedOptions
                .ElementAt(0);

            option
                .Arguments
                .Should()
                .BeEquivalentTo("argument1");
        }

        [Fact]
        public void Options_have_references_to_parent_commands()
        {
            var inner = parser.DefinedOptions["outer"]["inner"];
            var option = inner["option"];

            option.Parent.Should().Be(inner);
        }

        [Fact]
        public void Commands_have_references_to_parent_commands()
        {
            var outer = parser.DefinedOptions["outer"];
            var inner = outer["inner"];

            inner.Parent.Should().Be(outer);
        }

        [Fact]
        public void Commands_at_multiple_levels_can_have_their_own_arguments()
        {
            var parser = new Parser(
                Command("outer", "",
                        ExactlyOneArgument(),
                        Command("inner", "",
                                ZeroOrMoreArguments())));

            var result = parser.Parse("outer arg1 inner arg2 arg3");

            WriteLine(result);

            result["outer"]
                .Arguments
                .Should()
                .BeEquivalentTo("arg1");

            result["outer"]["inner"]
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
        public void ParseResult_Command_identifies_implicit_root_command()
        {
            var parser1 = new Parser(
                Option("-x", ""),
                Option("-y", ""));

            var result = parser1.Parse("-x -y");

            var command = result.Command();

            command.Should().NotBeNull();
            command.Name.Should().Be(RootCommand().Name);
        }

        [Fact]
        public void ParseResult_AppliedCommand_identifies_the_AppliedOption_for_the_innermost_command()
        {
            var command = Command("outer", "",
                                  Command("sibling", "",
                                          ExactlyOneArgument()),
                                  Command("inner", "",
                                          Command("inner-er", "",
                                                  Option("-x", "",
                                                         ExactlyOneArgument()))));

            var result = command.Parse("outer inner inner-er -x arg");

            var appliedOption = result.AppliedCommand()["x"];

            appliedOption.Value().Should().Be("arg");

            result = command.Parse("outer sibling arg");

            result.AppliedCommand().Value().Should().Be("arg");
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

            command.Parse("test ").Suggestions()
                   .Should().BeEquivalentTo("foo", "bar");
        }
    }
}