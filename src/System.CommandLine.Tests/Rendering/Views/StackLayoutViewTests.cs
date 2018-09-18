using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering.Views
{
    public class StackLayoutViewTests
    {
        [Fact]
        public void Vertical_stack_displays_content_stacked_on_top_of_each_other()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 10, 2), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick"),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("brown fox"));
        }

        [Fact]
        public void Vertical_stack_clips_content_when_region_is_not_tall_enough()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 10, 1), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick"));
        }

        [Fact]
        public void Vertical_stack_wraps_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 5, 4), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The  "),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("quick"),
                new TestConsole.CursorPositionChanged(new Point(0, 2)),
                new TestConsole.ContentWritten("brown"),
                new TestConsole.CursorPositionChanged(new Point(0, 3)),
                new TestConsole.ContentWritten("fox  ")
                );
        }

        [Fact]
        public void Horizontal_stack_displays_content_stacked_on_next_to_each_other()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 18, 1), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick         "),
                new TestConsole.CursorPositionChanged(new Point(9, 0)),
                new TestConsole.ContentWritten("brown fox"));
        }

        [Fact]
        public void Horizontal_stack_clips_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 16, 1), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick       "),
                new TestConsole.CursorPositionChanged(new Point(9, 0)),
                new TestConsole.ContentWritten("brown  "));
        }

        [Fact]
        public void Horizontal_stack_wraps_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            stackLayout.Render(new Region(0, 0, 14, 2), renderer);

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick     "),
                new TestConsole.CursorPositionChanged(new Point(9, 0)),
                new TestConsole.ContentWritten("brown"),
                new TestConsole.CursorPositionChanged(new Point(9, 1)),
                new TestConsole.ContentWritten("fox  ")
            );
        }
    }
}
