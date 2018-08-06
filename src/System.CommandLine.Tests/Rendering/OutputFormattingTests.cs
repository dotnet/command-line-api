using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;

namespace System.CommandLine.Tests.Rendering
{
    public class OutputFormattingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleRenderer _renderer;

        public OutputFormattingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            _renderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters()
        {
            _renderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            new ConsoleView<TimeSpan>(_renderer).Render(21.Seconds());

            _console.Out.ToString().TrimEnd().Should().Be("21 seconds");
        }

        [Fact]
        public void Type_formatters_apply_to_table_cells()
        {
            var view = new ProcessTimesView(_renderer);

            _renderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            view.Render(Example_TOP.Processes);

            _output.WriteLine(_console.Out.ToString());

            _console.Out.ToString().Should().Contain("42.82 seconds");
        }

        [Fact]
        public void FormattableString_can_contain_format_strings_that_reformat_the_input_value()
        {
            _renderer.Formatter
                           .AddFormatter<DateTime>(d => $"{d:d} {Color.Foreground.DarkGray}{d:t}{Color.Foreground.Default}");

            var dateTime = DateTime.Parse("8/2/2018 6pm");

            var span = _renderer.Formatter.Format(dateTime);

            span.ToString().Should().Be($"8/2/2018 {Color.Foreground.DarkGray}6:00 PM{Color.Foreground.Default}");
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
