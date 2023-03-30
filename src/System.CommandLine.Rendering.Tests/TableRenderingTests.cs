// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using static System.Environment;

namespace System.CommandLine.Rendering.Tests
{
    public class TableRenderingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestTerminal _terminal;

        public TableRenderingTests(ITestOutputHelper output)
        {
            _output = output;

            _terminal = new TestTerminal
            {
                Width = 150
            };
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column(OutputMode outputMode)
        {
            var options = new[] {
                new CliOption<string>("-s") { Description = "a short option" },
                new CliOption<string>("--very-long") { Description = "a long option" }
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_terminal, outputMode), new Region(0, 0, 30, 3));

            var lines = _terminal.RenderOperations();

            lines
                .Should()
                .BeEquivalentSequenceTo(
                    Cell("Option     ", 0, 0), Cell("  ", 11, 0), Cell("              ", 13, 0),
                    Cell("-s         ", 0, 1), Cell("  ", 11, 1), Cell("a short option", 13, 1),
                    Cell("--very-long", 0, 2), Cell("  ", 11, 2), Cell("a long option ", 13, 2));
        }

        [Fact]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column_in_file_mode()
        {
            var options = new[]
            {
                new CliOption<string>("-s") { Description = "a short option" },
                new CliOption<string>("--very-long") { Description = "a long option" }
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_terminal, OutputMode.PlainText), new Region(0, 0, 30, 3));

            _terminal.Out
                    .ToString()
                    .Should()
                    .Be(
                        "Option                     " + NewLine +
                        "-s           a short option" + NewLine +
                        "--very-long  a long option ");
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Column_widths_are_aligned_to_the_longest_cell(OutputMode outputMode)
        {
            var options = new[] {
                new CliOption<string>("-s") { Description = "an option" },
                new CliOption<string>("--very-long") { Description = "an option" },
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_terminal, outputMode), new Region(0,0, 30, 3));

            var lines = _terminal.RenderOperations()
                                .Select(l => l.Text)
                                .ToArray();

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }

        [Fact]
        public void Column_widths_are_aligned_to_the_longest_cell_in_file_mode()
        {
            var options = new[] {
                new CliOption<string>("-s") { Description = "an option" },
                new CliOption<string>("--very-long") { Description = "an option" },
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_terminal, OutputMode.PlainText), new Region(0,0, 30, 3));

            var lines = _terminal.Out.ToString()
                                .Split(NewLine)
                                .ToArray();

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }

        private TextRendered Cell(string text, int left, int top) => new(text, new Point(left, top));
    }

    public class OptionsHelpView : TableView<CliOption>
    {
        public OptionsHelpView(IEnumerable<CliOption> options)
        {
            Items = options.ToList();

            AddColumn(o => string.Join(", ", new[] { o.Name }.Concat(o.Aliases)), "Option");
            AddColumn(o => o.Description, "");
        }
    }
}
