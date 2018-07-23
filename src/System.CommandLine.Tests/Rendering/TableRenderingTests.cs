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
                WindowWidth = 150
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



        // __________________________________
        // | A     | .......                 |
        // | B     | .............           |
        // | C     | .......                 |
        // | D     | ....                    |

        // ___________________________
        // | A B C | The quick brown |
        // |       | fox jumps over  |
        // |       | the lazy dog    |
        // |       |                 | 



        [Fact]
        public void Text_can_be_wrapped_within_the_console_window()
        {
            var text = "The quick brown fox jumps over the lazy dog";
            _console.WindowWidth = 14;
            var view = new AnonymousView<string>(_consoleWriter, (arg, writer) => writer.Write(arg));
            view.Render(text);
            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out.ToString();

            lines.Should().Be($"The quick{NewLine}" +
                              $"brown fox{NewLine}" +
                              $"jumps over the{NewLine}" +
                              $"lazy dog");
        }

        [Fact]
        public void Text_wraps_exactly_to_the_window_width()
        {
            var text = "a a a a a";
            _console.WindowWidth = 5;
            var view = new AnonymousView<string>(_consoleWriter, (arg, writer) => writer.Write(arg));
            view.Render(text);
            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out.ToString();

            lines.Should().Be($"a a a{NewLine}" +
                              $"a a");
        }
    }

    internal class MutilineView : ConsoleView<string>
    {
        public MutilineView(IConsoleWriter writer, int height, int width) : base(writer)
        {
        }

        public override void Render(string value) => ConsoleWriter.Write(value);
    }

    public class OptionsHelpView : ConsoleView<IEnumerable<Option>>
    {
        public OptionsHelpView(IConsoleWriter writer) : base(writer)
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
