using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class TableRenderingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleWriter _consoleWriter;

        public TableRenderingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            _consoleWriter = new ConsoleWriter(_console);
        }

        [Fact]
        public void A_row_is_written_for_each_item_and_a_header_for_each_column()
        {
            var options = new[] {
                new Option("-s", "a short option"),
                new Option("--very-long", "a long option")
            };

            var view = new OptionsHelpView(_consoleWriter);

            view.Render(options);

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out.ToString().Split(NewLine);

            lines[0].Should().Contain("Option");
            lines[1].Should().Contain("-s");
            lines[2].Should().Contain("--very-long");
        }

        [Fact]
        public void Column_widths_are_aligned_to_the_longest_cell()
        {
            var options = new[] {
                new Option("-s", "an option"),
                new Option("--very-long", "an option")
            };

            var view = new OptionsHelpView(_consoleWriter);

            view.Render(options);

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out
                                .ToString()
                                .Split(NewLine);

            lines[1].IndexOf("an option")
                    .Should()
                    .Be(lines[2].IndexOf("an option"));
        }
    }

    public class OptionsHelpView : ConsoleView<IEnumerable<Option>>
    {
        public OptionsHelpView(ConsoleWriter writer) : base(writer)
        {
        }

        public override void Render(IEnumerable<Option> options)
        {
            ConsoleWriter.RenderTable(options.ToArray(),
                                      table => {
                                          table.RenderColumn("Option", o => string.Join(", ", o.RawAliases));
                                          table.RenderColumn("", o => o.Description);
                                      });
        }
    }
}
