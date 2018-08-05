using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class OutputFormattingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleRenderer consoleRenderer;

        public OutputFormattingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters()
        {
            consoleRenderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            new ConsoleView<TimeSpan>(consoleRenderer).Render(21.Seconds());

            _console.Out.ToString().TrimEnd().Should().Be("21 seconds");
        }

        [Fact]
        public void Type_formatters_apply_to_table_cells()
        {
            var view = new ProcessTimesView(consoleRenderer);

            consoleRenderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            view.Render(Example_TOP.Processes);

            _output.WriteLine(_console.Out.ToString());

            _console.Out.ToString().Should().Contain("42.82 seconds");
        }
    }

    public class ProcessTimesView : ConsoleView<IEnumerable<ProcessInfo>>
    {
        public ProcessTimesView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
        {
        }

        public override void Render(IEnumerable<ProcessInfo> processes)
        {
            RenderTable(
                items: processes,
                table => {
                    table.RenderColumn("COMMAND", p => p.Command);
                    table.RenderColumn("TIME", p => p.ExecutionTime);
                });
        }
    }
}
