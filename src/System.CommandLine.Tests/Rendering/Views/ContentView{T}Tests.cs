using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Drawing;
using FluentAssertions;
using Xunit;
using Size = System.CommandLine.Rendering.Size;

namespace System.CommandLine.Tests.Rendering.Views
{
    public class ContentViewGenericTests
    {
        private readonly TestConsole _console;
        private readonly IRenderer _renderer;

        public ContentViewGenericTests()
        {
            _console = new TestConsole();
            _renderer = new ConsoleRenderer(_console);
        }

        [Fact]
        public void Measure_creates_formatted_span()
        {
            var view = new ContentView<int>(421);

            Size measuredSize = view.Measure(_renderer, new Size(10, 1));
            measuredSize.Width.Should().Be(3);
            measuredSize.Height.Should().Be(1);
        }

        [Fact]
        public void Render_outputs_formatted_span()
        {
            var view = new ContentView<int>(421);

            view.Render(_renderer, new Region(0, 0, 3, 1));

            _console.Events.Should().BeEquivalentTo(new object[]
            {
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("421")
            }, config => config.WithStrictOrdering());
        }

        [Fact]
        public void Span_is_only_created_after_measure_is_called()
        {
            var view = new TestableContentView<int>(42);

            view.IsSpanCreated.Should().BeFalse();
            view.Measure(_renderer, new Size(0, 0));
            view.IsSpanCreated.Should().BeTrue();
        }

        [Fact]
        public void Span_is_only_created_after_render_is_called()
        {
            var view = new TestableContentView<int>(42);

            view.IsSpanCreated.Should().BeFalse();
            view.Render(_renderer, new Region(0, 0, 0, 0));
            view.IsSpanCreated.Should().BeTrue();
        }

        private class TestableContentView<T> : ContentView<T>
        {
            public bool IsSpanCreated => Span != null;

            public TestableContentView(T value)
                : base(value)
            { }
        }
    }
}
