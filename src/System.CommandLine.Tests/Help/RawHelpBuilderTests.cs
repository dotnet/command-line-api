// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public class RawHelpBuilderTests
    {
        private readonly RawHelpBuilder _rawHelpBuilder;
        private readonly IConsole _console;
        private readonly ITestOutputHelper _output;
        private const string ExecutableName = "testhost";
        private const int MaxWidth = 60;
        private const int ColumnGutterWidth = 4;
        private const int IndentationWidth = 2;
        private readonly string _columnPadding;
        private readonly string _indentation;

        public RawHelpBuilderTests(ITestOutputHelper output)
        {
            _console = new TestConsole();
            _rawHelpBuilder = new RawHelpBuilder(
                console: _console,
                columnGutter: ColumnGutterWidth,
                indentationSize: IndentationWidth,
                maxWidth: MaxWidth
            );

            _output = output;
            _columnPadding = new string(' ', ColumnGutterWidth);
            _indentation = new string(' ', IndentationWidth);
        }

        public string GetHelpText()
        {
            return _console.Out.ToString();
        }

        #region " Synopsis "

        [Fact]
        public void Whitespace_is_preserved_in_synopsis()
        {
            var command = new CommandLineBuilder
            {
                HelpBuilder = _rawHelpBuilder,
                Description = "test  description\tfor synopsis",
            }
            .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test  description{"\t"}for synopsis

Usage:
{_indentation}{ExecutableName}

";
            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_synopsis()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = $"test{NewLine}description with{NewLine}line breaks",
                }
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test
{_indentation}description with
{_indentation}line breaks

Usage:
{_indentation}{ExecutableName}

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_synopsis()
        {
            var longText = $"test{NewLine}description with line breaks that is long enough to wrap to a{NewLine}new line";
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = longText,
                }
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test
{_indentation}description with line breaks that is long enough to wrap
{_indentation} to a
{_indentation}new line

Usage:
{_indentation}{ExecutableName}

";
            GetHelpText().Should().Be(expected);
        }

        #endregion " Synopsis "

        #region " Usage "

        [Fact]
        public void Whitespace_is_preserved_in_usage()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", "Help text   for the outer   command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test  description

Usage:
{_indentation}{ExecutableName} [command]

Commands:
{_indentation}outer{_columnPadding}Help text   for the outer   command

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_usage()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", $"Help text{NewLine}for the outer{NewLine}command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test  description

Usage:
{_indentation}{ExecutableName} [command]

Commands:
{_indentation}outer{_columnPadding}Help text
{_indentation}     {_columnPadding}for the outer
{_indentation}     {_columnPadding}command

";
            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_usage()
        {
            var longText = $"Help{NewLine}text with line breaks that is long enough to wrap to a{NewLine}new line";
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", longText,
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            var expected = $@"{ExecutableName}:
{_indentation}test  description

Usage:
{_indentation}{ExecutableName} [command]

Commands:
{_indentation}outer{_columnPadding}Help
{_indentation}     {_columnPadding}text with line breaks that is long enough to{" "}
{_indentation}     {_columnPadding}wrap to a
{_indentation}     {_columnPadding}new line

";
            GetHelpText().Should().Be(expected);
        }

        #endregion " Usage "

        #region " Arguments "

        [Fact]
        public void Whitespace_is_preserved_in_arguments()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: "Argument\tfor the   inner command")
                        .ExactlyOne())
                .BuildCommandDefinition();
            command.Subcommand("outer").GenerateHelp(_console);

            var expected = $@"outer:
{_indentation}Help text for the outer command

Usage:
{_indentation}{ExecutableName} outer <outer-command-arg>

Arguments:
{_indentation}<outer-command-arg>{_columnPadding}Argument{"\t"}for the   inner command

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_arguments()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: $"The argument{NewLine}for the{NewLine}inner command")
                        .ExactlyOne())
                .BuildCommandDefinition();
            command.Subcommand("outer").GenerateHelp(_console);

            var expected = $@"outer:
  Help text for the outer command

Usage:
{_indentation}{ExecutableName} outer <outer-command-arg>

Arguments:
{_indentation}<outer-command-arg>{_columnPadding}The argument
{_indentation}                   {_columnPadding}for the
{_indentation}                   {_columnPadding}inner command

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_arguments()
        {
            var longText = $"The argument{NewLine}for the inner command with line breaks that is long enough to wrap to a{NewLine}new line";
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("outer", "Help text for the outer command",
                    arguments: args => args
                        .WithHelp(
                            name: "outer-command-arg",
                            description: longText)
                        .ExactlyOne())
                .BuildCommandDefinition();
            command.Subcommand("outer").GenerateHelp(_console);

            var expected = $@"outer:
  Help text for the outer command

Usage:
{_indentation}{ExecutableName} outer <outer-command-arg>

Arguments:
{_indentation}<outer-command-arg>{_columnPadding}The argument
{_indentation}                   {_columnPadding}for the inner command with line{" "}
{_indentation}                   {_columnPadding}breaks that is long enough to{" "}
{_indentation}                   {_columnPadding}wrap to a
{_indentation}                   {_columnPadding}new line

";

            GetHelpText().Should().Be(expected);
        }

        #endregion " Arguments "

        #region " Options "

        [Fact]
        public void Whitespace_is_preserved_in_options()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            "Help   for      the   option"))
                .BuildCommandDefinition();
            command.Subcommand("test-command").GenerateHelp(_console);

            var expected = $@"test-command:
{_indentation}Help text for the command

Usage:
{_indentation}{ExecutableName} test-command [options]

Options:
{_indentation}-a, --aaa{_columnPadding}Help   for      the   option

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_options()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            $"Help{NewLine}for {NewLine} the{NewLine}option"))
                .BuildCommandDefinition();
            command.Subcommand("test-command").GenerateHelp(_console);

            var expected = $@"test-command:
{_indentation}Help text for the command

Usage:
{_indentation}{ExecutableName} test-command [options]

Options:
{_indentation}-a, --aaa{_columnPadding}Help
{_indentation}         {_columnPadding}for{" "}
{_indentation}         {_columnPadding} the
{_indentation}         {_columnPadding}option

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_options()
        {
            var longText = $"The option{NewLine}with line breaks that is long enough to wrap to a{NewLine}new line";
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            longText))
                .BuildCommandDefinition();
            command.Subcommand("test-command").GenerateHelp(_console);

            var expected = $@"test-command:
{_indentation}Help text for the command

Usage:
{_indentation}{ExecutableName} test-command [options]

Options:
{_indentation}-a, --aaa{_columnPadding}The option
{_indentation}         {_columnPadding}with line breaks that is long enough to{" "}
{_indentation}         {_columnPadding}wrap to a
{_indentation}         {_columnPadding}new line

";

            GetHelpText().Should().Be(expected);
        }

        #endregion " Options "

        #region " Subcommands "

        [Fact]
        public void Whitespace_is_preserved_in_subcommands()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
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
                .BuildCommandDefinition();

            command.Subcommand("outer-command").Subcommand("inner-command").GenerateHelp(_console);

            var expected = $@"inner-command:
{_indentation}inner    command{"\t"} help  with whitespace

Usage:
{_indentation}{ExecutableName} outer-command <outer-args> inner-command{" "}
{_indentation}[options] <inner-args>

Arguments:
  <outer-args>{_columnPadding}
  <inner-args>{_columnPadding}

Options:
{_indentation}-v, --verbosity{_columnPadding}Inner    option {"\t"}with spaces

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_subcommands()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", $"inner{NewLine}command help {NewLine} with {NewLine}newlines",
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            $"Inner {NewLine} command {NewLine}option with{NewLine} newlines")))
                .BuildCommandDefinition();

            command.Subcommand("outer-command").Subcommand("inner-command").GenerateHelp(_console);

            var expected = $@"inner-command:
{_indentation}inner
{_indentation}command help{" "}
{_indentation} with{" "}
{_indentation}newlines

Usage:
{_indentation}{ExecutableName} outer-command <outer-args> inner-command{" "}
{_indentation}[options] <inner-args>

Arguments:
  <outer-args>{_columnPadding}
  <inner-args>{_columnPadding}

Options:
{_indentation}-v, --verbosity{_columnPadding}Inner{" "}
{_indentation}               {_columnPadding} command{" "}
{_indentation}               {_columnPadding}option with
{_indentation}               {_columnPadding} newlines

";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_subcommands()
        {
            var longText = $"The{NewLine}subcommand with line breaks that is long enough to wrap to a{NewLine}new line";
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand(
                    "outer-command", "outer command help",
                    arguments: outerArgs => outerArgs
                        .WithHelp(name: "outer-args")
                        .ZeroOrMore(),
                    symbols: outer => outer.AddCommand(
                        "inner-command", longText,
                        arguments: args => args
                            .WithHelp(name: "inner-args")
                            .ZeroOrOne(),
                        symbols: inner => inner.AddOption(
                            new[] { "-v", "--verbosity" },
                            longText)))
                .BuildCommandDefinition();

            command.Subcommand("outer-command").Subcommand("inner-command").GenerateHelp(_console);

            var expected = $@"inner-command:
{_indentation}The
{_indentation}subcommand with line breaks that is long enough to wrap{" "}
{_indentation}to a
{_indentation}new line

Usage:
{_indentation}{ExecutableName} outer-command <outer-args> inner-command{" "}
{_indentation}[options] <inner-args>

Arguments:
  <outer-args>{_columnPadding}
  <inner-args>{_columnPadding}

Options:
{_indentation}-v, --verbosity{_columnPadding}The
{_indentation}               {_columnPadding}subcommand with line breaks that is{" "}
{_indentation}               {_columnPadding}long enough to wrap to a
{_indentation}               {_columnPadding}new line

";

            GetHelpText().Should().Be(expected);
        }

        #endregion " Subcommands "
    }
}
