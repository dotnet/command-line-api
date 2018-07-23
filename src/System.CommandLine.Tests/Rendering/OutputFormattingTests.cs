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
        private readonly ConsoleWriter _consoleWriter;

        public OutputFormattingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                WindowWidth = 150
            };

            _consoleWriter = new ConsoleWriter(_console);
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters()
        {
            _consoleWriter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            View<TimeSpan>().Render(21.Seconds());

            _console.Out.ToString().Should().Be("21 seconds");
        }

        [Fact]
        public void Type_formatters_apply_to_table_cells()
        {
            var view = View<IEnumerable<ProcessInfo>>((processes, writer) => {
                writer.RenderTable(
                    items: processes,
                    table => {
                        table.RenderColumn("COMMAND", p => p.Command);
                        table.RenderColumn("TIME", p => p.ExecutionTime);
                    });
            });

            _consoleWriter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            view.Render(Example_TOP.Processes);

            _output.WriteLine(_console.Out.ToString());

            _console.Out.ToString().Should().Contain("42.82 seconds");


            

            // COMMAND        TIME               
            // Terminal       00:00:42.8200000   
            // WindowServer   20:04:23           
            // top            00:00:07.9500000   
            // kernel_task    17:05:49           
            // mds_stores     00:54:45.1500000   
            // mdworker       00:00:17.7200000   
            // hidd           00:53:33.1200000   
            // coreaudiod     00:58:14.2600000   

        }

        private AnonymousView<T> View<T>(Action<T, IConsoleWriter> render = null)
        {
            render = render ?? ((value, writer) => writer.Write(value));

            return new AnonymousView<T>(
                _consoleWriter,
                render);
        }
    }
}