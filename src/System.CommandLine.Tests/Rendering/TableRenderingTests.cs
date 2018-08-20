using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class TableRenderingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleRenderer consoleRenderer;

        public TableRenderingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column()
        {
            var options = new[] {
                new Option("-s", "a short option"),
                new Option("--very-long", "a long option")
            };

            var view = new OptionsHelpView(consoleRenderer);

            view.Render(options);

            var lines = _console.OutputLines();

            lines
                .Should()
                .BeEquivalentTo(
                    new[] {
                        Cell("Option       ", 0, 0), Cell("                ", 13, 0),
                        Cell("-s           ", 0, 1), Cell("a short option  ", 13, 1),
                        Cell("--very-long  ", 0, 2), Cell("a long option   ", 13, 2),
                    }, o => o.WithStrictOrdering());
        }

     private   TextRendered Cell(string text, int left, int top) =>             new TextRendered(text, new Point(left, top));

        [Fact]
        public void Column_widths_are_aligned_to_the_longest_cell()
        {
            var options = new[] {
                new Option("-s", "an option"),
                new Option("--very-long", "an option")
            };

            var view = new OptionsHelpView(consoleRenderer);

            view.Render(options);

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.OutputLines()
                                .Select(l => l.Text)
                                .ToArray();

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }
    }

    public class OptionsHelpView : ConsoleView<IEnumerable<Option>>
    {
        public OptionsHelpView(ConsoleRenderer renderer) : base(renderer)
        {
        }

        protected override void OnRender(IEnumerable<Option> options)
        {
            RenderTable(
                options.ToArray(),
                table => {
                    table.RenderColumn("Option", o => string.Join(", ", o.RawAliases));
                    table.RenderColumn("", o => o.Description);
                });
        }
    }
}
