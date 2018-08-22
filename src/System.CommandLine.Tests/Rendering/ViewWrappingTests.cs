using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;

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
        public void Text_wraps_within_the_console_window_when_region_is_not_specified()
        {
            var text = "The quick brown fox jumps over the lazy dog";

            _console.Width = 14;
            _console.Height = 4;

            var view = new StringView(consoleRenderer);

            view.Render(text);

            _console.RenderOperations()
                    .Select(l => l.Text)
                    .Should()
                    .BeEquivalentTo(
                        new[] {
                            "The quick     ",
                            "brown fox     ",
                            "jumps over the",
                            "lazy dog      "
                        },
                        options => options.WithStrictOrdering());
        }

        [Fact]
        public void Text_wraps_within_the_specified_region()
        {
            var text = "1 1 1 2 2";

            var view = new StringView(
                consoleRenderer,
                new Region(0, 0, 5, 2));

            view.Render(text);

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

        private class StringView : ConsoleView<string>
        {
            public StringView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
            {
            }

            protected override void OnRender(string value) => Write(value);
        }
    }
}
