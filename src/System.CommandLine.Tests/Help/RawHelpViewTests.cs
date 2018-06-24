// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Help
{
    public class RawHelpViewTests
    {
        private readonly RawHelpBuilder _rawHelpBuilder;
        private readonly IConsole _console;
        private readonly ITestOutputHelper _output;
        private const string ExecutableName = "testhost";
        private const int MaxWidth = 80;
        private const int ColumnGutterWidth = 4;
        private const int IndentationWidth = 2;
        private readonly string _columnPadding;
        private readonly string _indentation;
        private readonly StringBuilder _expected;

        public RawHelpViewTests(ITestOutputHelper output)
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
            _expected = new StringBuilder();
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
                Description = "test  description",
            }
            .BuildCommandDefinition();
            command.GenerateHelp(_console);

            _expected.AppendLine($"{ExecutableName}:");
            _expected.AppendLine($"{_indentation}test  description{NewLine}");
            _expected.AppendLine("Usage:");
            _expected.AppendLine($"{_indentation}{ExecutableName}{NewLine}");

            var help = GetHelpText();
            _output.WriteLine(help);

            help.Should().Be(_expected.ToString());
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

            _expected.AppendLine($"{ExecutableName}:");
            _expected.AppendLine($"{_indentation}test");
            _expected.AppendLine($"{_indentation}description with");
            _expected.AppendLine($"{_indentation}line breaks{NewLine}");
            _expected.AppendLine("Usage:");
            _expected.AppendLine($"{_indentation}{ExecutableName}{NewLine}");

            var help = GetHelpText();
            _output.WriteLine(help);

            help.Should().Be(_expected.ToString());
        }

        #endregion " Synopsis "

        #region " Usage "

        [Fact]
        public void Whitespace_is_preserved_in_usage()
        {
            var command = new CommandLineBuilder
                {
                    HelpBuilder = _rawHelpBuilder,
                    Description = "test  description"
                }
                .AddCommand("outer", "Help text   for the outer   command",
                    arguments: args => args.ExactlyOne())
                .BuildCommandDefinition();
            command.GenerateHelp(_console);

            _expected.AppendLine($"{ExecutableName}:");
            _expected.AppendLine($"{_indentation}test  description{NewLine}");
            _expected.AppendLine("Usage:");
            _expected.AppendLine($"{_indentation}{ExecutableName} [command]{NewLine}");
            _expected.AppendLine("Commands:");
            _expected.AppendLine($"{_indentation}outer{_columnPadding}Help text   for the outer   command{NewLine}");

            var help = GetHelpText();
            _output.WriteLine(help);

            help.Should().Be(_expected.ToString());
        }

        #endregion " Usage "
    }
}
