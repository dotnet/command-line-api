using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using System.CommandLine.Rendering.Views;

namespace System.CommandLine.Tests.Rendering
{
    public class ViewWrappingTests
    {
        private readonly TestConsole _console;
        private readonly ConsoleRenderer consoleRenderer;

        public ViewWrappingTests()
        {
            _console = new TestConsole {
                Width = 150
            };

            consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void Text_wraps_within_the_specified_region()
        {
            var text = "1 1 1 2 2";

            var view = new ContentView(text);

            view.Render(consoleRenderer,
                new Region(0, 0, 5, 2));

            _console.RenderOperations()
                    .Select(l => l.Text)
                    .Should()
                    .BeEquivalentTo(
                        new[] {
                            "1 1 1",
                            "2 2  "
                        },
                        options => options.WithStrictOrdering());
        }
    }
}
