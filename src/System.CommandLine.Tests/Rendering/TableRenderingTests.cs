using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.CommandLine.Rendering.Views;

namespace System.CommandLine.Tests.Rendering
{
    public class TableRenderingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleRenderer _consoleRenderer;

        public TableRenderingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole
            {
                Width = 150
            };

            _consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column()
        {
            var options = new[] {
                new Option("-s", "a short option"),
                new Option("--very-long", "a long option")
            };

            var view = new OptionsHelpView(options);

            view.Render(_consoleRenderer, new Region(0, 0, 30, 3));

            var lines = _console.RenderOperations();

            lines
                .Should()
                .BeEquivalentSequenceTo(
                    Cell("Option     ", 0, 0), Cell("              ", 11, 0),
                    Cell("-s         ", 0, 1), Cell("a short option", 11, 1),
                    Cell("--very-long", 0, 2), Cell("a long option ", 11, 2));
        }

        private TextRendered Cell(string text, int left, int top) => new TextRendered(text, new Point(left, top));

        [Fact]
        public void Column_widths_are_aligned_to_the_longest_cell()
        {
            var options = new[] {
                new Option("-s", "an option"),
                new Option("--very-long", "an option")
            };

            var view = new OptionsHelpView(options);

            view.Render(_consoleRenderer, new Region(0,0, 30, 3));

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.RenderOperations()
                                .Select(l => l.Text)
                                .ToArray();

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }
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
