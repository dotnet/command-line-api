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
        private const int MaxWidth = 60;
        private const int ColumnGutterWidth = 4;
        private const int IndentationWidth = 2;

        private readonly RawHelpBuilder _rawHelpBuilder;
        private readonly IConsole _console;
        private readonly ITestOutputHelper _output;
        private readonly string _executableName;
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
            _executableName = CommandLineBuilder.ExeName;
        }

        private string GetHelpText()
        {
            return _console.Out.ToString();
        }

        #region Synopsis

        [Fact]
        public void Extra_whitespace_is_preserved_in_synopsis()
        {
            var commandLineBuilder = new CommandLineBuilder
            {
                HelpBuilder = _rawHelpBuilder,
                Description = "test  description\tfor synopsis",
            }
            .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test  description\tfor synopsis{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_synopsis()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = $"test{NewLine}description with{NewLine}line breaks",
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test{NewLine}" +
            $"{_indentation}description with{NewLine}" +
            $"{_indentation}line breaks{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_synopsis()
        {
            var longText = $"test{NewLine}description with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = longText,
                }
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test{NewLine}" +
            $"{_indentation}description with line breaks that is long enough to wrap{NewLine}" +
            $"{_indentation} to a{NewLine}" +
            $"{_indentation}new line{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName}{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Synopsis

        #region Usage

        [Fact]
        public void Extra_whitespace_is_preserved_in_usage()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", "Help text   for the outer   command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test  description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help text   for the outer   command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_usage()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", $"Help text{NewLine}for the outer{NewLine}command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test  description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help text{NewLine}" +
            $"{_indentation}     {_columnPadding}for the outer{NewLine}" +
            $"{_indentation}     {_columnPadding}command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_usage()
        {
            var longText = $"Help{NewLine}text with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description",
                }
                .AddCommand("outer", longText,
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();

            commandLineBuilder.WriteHelp(_console);

            var expected =
            $"{_executableName}:{NewLine}" +
            $"{_indentation}test  description{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} [command]{NewLine}{NewLine}" +
            $"Commands:{NewLine}" +
            $"{_indentation}outer{_columnPadding}Help{NewLine}" +
            $"{_indentation}     {_columnPadding}text with line breaks that is long enough to {NewLine}" +
            $"{_indentation}     {_columnPadding}wrap to a{NewLine}" +
            $"{_indentation}     {_columnPadding}new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Usage

        #region Arguments

        [Fact]
        public void Extra_whitespace_is_preserved_in_arguments()
        {
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}Argument{"\t"}for the   inner command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_arguments()
        {
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}The argument{NewLine}" +
            $"{_indentation}                   {_columnPadding}for the{NewLine}" +
            $"{_indentation}                   {_columnPadding}inner command{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_arguments()
        {
            var longText = $"The argument{NewLine}for the inner command with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer")
                .WriteHelp(_console);

            var expected =
            $"outer:{NewLine}" +
            $"{_indentation}Help text for the outer command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer <outer-command-arg>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-command-arg>{_columnPadding}The argument{NewLine}" +
            $"{_indentation}                   {_columnPadding}for the inner command with line {NewLine}" +
            $"{_indentation}                   {_columnPadding}breaks that is long enough to {NewLine}" +
            $"{_indentation}                   {_columnPadding}wrap to a{NewLine}" +
            $"{_indentation}                   {_columnPadding}new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Arguments

        #region Options

        [Fact]
        public void Extra_whitespace_is_preserved_in_options()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            "Help   for      the   option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help   for      the   option{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_options()
        {
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            $"Help{NewLine}for {NewLine} the{NewLine}option"))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}Help{NewLine}" +
            $"{_indentation}         {_columnPadding}for {NewLine}" +
            $"{_indentation}         {_columnPadding} the{NewLine}" +
            $"{_indentation}         {_columnPadding}option{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_options()
        {
            var longText = $"The option{NewLine}with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                }
                .AddCommand("test-command", "Help text for the command",
                    symbols => symbols
                        .AddOption(
                            new[] { "-a", "--aaa" },
                            longText))
                .BuildCommandDefinition();

            commandLineBuilder
                .Subcommand("test-command")
                .WriteHelp(_console);

            var expected =
            $"test-command:{NewLine}" +
            $"{_indentation}Help text for the command{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} test-command [options]{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-a, --aaa{_columnPadding}The option{NewLine}" +
            $"{_indentation}         {_columnPadding}with line breaks that is long enough to {NewLine}" +
            $"{_indentation}         {_columnPadding}wrap to a{NewLine}" +
            $"{_indentation}         {_columnPadding}new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Options

        #region Subcommands

        [Fact]
        public void Extra_whitespace_is_preserved_in_subcommands()
        {
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $"inner-command:{NewLine}" +
            $"{_indentation}inner    command\t help  with whitespace{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command {NewLine}" +
            $"{_indentation}[options] <inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}Inner    option \twith spaces{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Newlines_are_preserved_in_subcommands()
        {
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $"inner-command:{NewLine}" +
            $"{_indentation}inner{NewLine}" +
            $"{_indentation}command help {NewLine}" +
            $"{_indentation} with {NewLine}" +
            $"{_indentation}newlines{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command {NewLine}" +
            $"{_indentation}[options] <inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}Inner {NewLine}" +
            $"{_indentation}               {_columnPadding} command {NewLine}" +
            $"{_indentation}               {_columnPadding}option with{NewLine}" +
            $"{_indentation}               {_columnPadding} newlines{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        [Fact]
        public void Format_is_preserved_with_wrapping_in_subcommands()
        {
            var longText = $"The{NewLine}subcommand with line breaks that is long enough to wrap to a{NewLine}new line";
            var commandLineBuilder = new CommandLineBuilder
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

            commandLineBuilder
                .Subcommand("outer-command")
                .Subcommand("inner-command")
                .WriteHelp(_console);

            var expected =
            $"inner-command:{NewLine}" +
            $"{_indentation}The{NewLine}" +
            $"{_indentation}subcommand with line breaks that is long enough to wrap {NewLine}" +
            $"{_indentation}to a{NewLine}" +
            $"{_indentation}new line{NewLine}{NewLine}" +
            $"Usage:{NewLine}" +
            $"{_indentation}{_executableName} outer-command <outer-args> inner-command {NewLine}" +
            $"{_indentation}[options] <inner-args>{NewLine}{NewLine}" +
            $"Arguments:{NewLine}" +
            $"{_indentation}<outer-args>{_columnPadding}{NewLine}" +
            $"{_indentation}<inner-args>{_columnPadding}{NewLine}{NewLine}" +
            $"Options:{NewLine}" +
            $"{_indentation}-v, --verbosity{_columnPadding}The{NewLine}" +
            $"{_indentation}               {_columnPadding}subcommand with line breaks that is {NewLine}" +
            $"{_indentation}               {_columnPadding}long enough to wrap to a{NewLine}" +
            $"{_indentation}               {_columnPadding}new line{NewLine}{NewLine}";

            GetHelpText().Should().Be(expected);
        }

        #endregion Subcommands
    }
}
