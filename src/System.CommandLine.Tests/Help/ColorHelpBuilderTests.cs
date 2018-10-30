// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Help
{
    public class ColorHelpBuilderTests
    {
        private class ColorTestConsole : TestConsole
        {
            private ConsoleColor _foregroundColor;

            public int ForegroundColorCalls { get; private set; }

            public int ResetColorCalls { get; private set; }

            public override ConsoleColor ForegroundColor
            {
                get => _foregroundColor;
                set
                {
                    ForegroundColorCalls += 1;
                    _foregroundColor = value;
                }
            }

            public override void ResetColor()
            {
                ResetColorCalls += 1;
            }
        }

        private readonly ColorHelpBuilder _colorHelpBuilder;

        private readonly ColorTestConsole _console;

        public ColorHelpBuilderTests()
        {
            _console = new ColorTestConsole();
            _colorHelpBuilder = new ColorHelpBuilder(_console);
        }

        #region Synopsis

        [Fact]
        public void Synopsis_sets_and_resets_foreground_color()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _colorHelpBuilder,
                Description = "test description for synopsis",
            }
            .BuildCommand();

            commandLineBuilder.WriteHelp(_console);

            _console.ForegroundColorCalls.Should().Be(2);
            _console.ResetColorCalls.Should().Be(2);
        }

        #endregion Synopsis

        #region Usage

        [Fact]
        public void Usage_section_sets_and_resets_foreground_color()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _colorHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", "Help text   for the outer   command",
                    arguments: args => args.ExactlyOne())
                .BuildCommand();

            command.WriteHelp(_console);

            _console.ForegroundColorCalls.Should().Be(3);
            _console.ResetColorCalls.Should().Be(3);
        }

        #endregion Usage

        #region Arguments

        [Fact]
        public void Arguments_section_sets_and_resets_foreground_color()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _colorHelpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: "Argument\tfor the   inner command")
                        .ExactlyOne())
                .BuildCommand();

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            _console.ForegroundColorCalls.Should().Be(3);
            _console.ResetColorCalls.Should().Be(3);
        }

        #endregion Arguments

        #region Options

        [Fact]
        public void Options_section_sets_and_resets_foreground_color()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _colorHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            "Help   for      the   option"))
                .BuildCommand();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            _console.ForegroundColorCalls.Should().Be(3);
            _console.ResetColorCalls.Should().Be(3);
        }

        #endregion Options

        #region Subcommands

        [Fact]
        public void Subcommands_section_sets_and_resets_foreground_color()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _colorHelpBuilder,
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", "inner    command\t help  with whitespace",
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            "Inner    option \twith spaces")))
                .BuildCommand();

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            _console.ForegroundColorCalls.Should().Be(4);
            _console.ResetColorCalls.Should().Be(4);
        }

        #endregion Subcommands
    }
}
