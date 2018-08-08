using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class ViewWrappingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleRenderer consoleRenderer;

        public ViewWrappingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void Text_wraps_within_the_console_window_when_region_is_not_specified()
        {
            var text = "The quick brown fox jumps over the lazy dog";

            _console.Width = 14;
            _console.Height = 4;

            var view = new ConsoleView<string>(consoleRenderer);

            view.Render(text);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"The quick     {NewLine}" +
                        $"brown fox     {NewLine}" +
                        $"jumps over the{NewLine}" +
                        $"lazy dog      ");
        }

        [Fact]
        public void Text_wraps_within_the_specified_region()
        {
            var text = "1 1 1 2 2";

            var view = new ConsoleView<string>(
                consoleRenderer,
                new Region(0, 0, 5, 2));

            view.Render(text);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"1 1 1{NewLine}" +
                        $"2 2  ");
        }
    }
}
