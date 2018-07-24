using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class LayoutTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleWriter _consoleWriter;

        public LayoutTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            _consoleWriter = new ConsoleWriter(_console);
        }

        [Fact]
        public void Text_can_be_wrapped_within_the_console_window()
        {
            var text = "The quick brown fox jumps over the lazy dog";

            _console.Width = 14;

            var view = new ConsoleView<string>(_consoleWriter);

            view.Render(text);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"The quick     {NewLine}" +
                        $"brown fox     {NewLine}" +
                        $"jumps over the{NewLine}" +
                        $"lazy dog");
        }

        [Fact]
        public void Text_wraps_exactly_to_the_window_width()
        {
            var text = "1 1 1 2 2";

            var view = new ConsoleView<string>(
                _consoleWriter,
                new Region(2, 5, 0, 0));

            view.Render(text);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"1 1 1{NewLine}" +
                        $"2 2");
        }
    }
}
