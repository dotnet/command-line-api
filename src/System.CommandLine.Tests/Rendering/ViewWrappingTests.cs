using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class ViewWrappingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleWriter _consoleWriter;

        public ViewWrappingTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole {
                Width = 150
            };

            _consoleWriter = new ConsoleWriter(_console);
        }

        [Fact]
        public void Text_wraps_within_the_console_window_when_region_is_not_specified()
        {
            var text = "The quick brown fox jumps over the lazy dog";

            _console.Width = 14;

            var view = new ConsoleView<string>(_consoleWriter);

            view.Render(text);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"The quick     {Environment.NewLine}" +
                        $"brown fox     {Environment.NewLine}" +
                        $"jumps over the{Environment.NewLine}" +
                        $"lazy dog      ");
        }

        [Fact]
        public void Text_wraps_within_the_specified_region()
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
                    .Be($"1 1 1{Environment.NewLine}" +
                        $"2 2  ");
        }
    }
}