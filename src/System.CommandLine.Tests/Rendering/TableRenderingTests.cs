using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.CommandLine.Rendering.Views;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class TableRenderingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public TableRenderingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole
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
                new Option("-s", "a short option"),
                new Option("--very-long", "a long option")
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_console, outputMode), new Region(0, 0, 30, 3));

            var lines = _console.RenderOperations();

            lines
                .Should()
                .BeEquivalentSequenceTo(
                    Cell("Option       ", 0, 0), Cell("              ", 13, 0),
                    Cell("-s           ", 0, 1), Cell("a short option", 13, 1),
                    Cell("--very-long  ", 0, 2), Cell("a long option ", 13, 2));
        }

        [Fact]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column_in_file_mode()
        {
            var options = new[]
                          {
                              new Option("-s", "a short option"),
                              new Option("--very-long", "a long option")
                          };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_console, OutputMode.File), new Region(0, 0, 30, 3));

            _console.Out
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
                new Option("-s", "an option"),
                new Option("--very-long", "an option")
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_console, outputMode), new Region(0,0, 30, 3));

            var lines = _console.RenderOperations()
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
                new Option("-s", "an option"),
                new Option("--very-long", "an option")
            };

            var view = new OptionsHelpView(options);

            view.Render(new ConsoleRenderer(_console, OutputMode.File), new Region(0,0, 30, 3));

            var lines = _console.Out.ToString()
                                .Split(NewLine)
                                .ToArray();

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }

        private TextRendered Cell(string text, int left, int top) => new TextRendered(text, new Point(left, top));
    }

    public class OptionsHelpView : TableView<Option>
    {
        public OptionsHelpView(IEnumerable<Option> options)
        {
            Items = options.ToList();

            AddColumn(o => string.Join(", ", o.RawAliases), "Option");
            AddColumn(o => o.Description, "");
        }
    }
}
