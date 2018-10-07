using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Drawing;
using FluentAssertions;
using Xunit;
using Size = System.CommandLine.Rendering.Size;

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
            stackLayout.Render(renderer, new Region(0, 0, 10, 2));

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
            stackLayout.Render(renderer, new Region(0, 0, 10, 1));

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
            stackLayout.Render(renderer, new Region(0, 0, 5, 4));

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
            stackLayout.Render(renderer, new Region(0, 0, 18, 1));

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
            stackLayout.Render(renderer, new Region(0, 0, 16, 1));

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
            stackLayout.Render(renderer, new Region(0, 0, 14, 2));

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick     "),
                new TestConsole.CursorPositionChanged(new Point(9, 0)),
                new TestConsole.ContentWritten("brown"),
                new TestConsole.CursorPositionChanged(new Point(9, 1)),
                new TestConsole.ContentWritten("fox  ")
            );
        }

        [Fact]
        public void Measuring_a_vertical_stack_sums_content_height()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(10, 10));

            size.Should().BeEquivalentTo(new Size(9, 2));
        }

        [Fact]
        public void Measuring_a_vertical_stack_with_word_wrap_it_sums_max_height_for_each_row()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(7, 10));

            size.Should().BeEquivalentTo(new Size("brown ".Length, 4));
        }

        [Fact]
        public void Measuring_a_vertical_stack_with_row_truncation_the_top_row_is_measured_first()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(7, 1));

            var firstViewTopRow = "The ".Length;
            size.Should().BeEquivalentTo(new Size(firstViewTopRow, 1));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_sums_content_width()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(20, 20));

            size.Should().BeEquivalentTo(new Size("The quickbrown fox".Length, 1));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_with_word_wrap_it_sums_max_width_for_each_column()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(7, 10));

            var colOneWidth = Math.Max("The ".Length, "quick".Length);
            var colTwoWidth = Math.Max("brown ".Length, "fox".Length);
            size.Should().BeEquivalentTo(new Size(colOneWidth + colTwoWidth, 2));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_with_truncated_height_measures_max_for_each_column()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.AddChild(child1);
            stackLayout.AddChild(child2);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            
            var size = stackLayout.Measure(renderer, new Size(7, 1));

            size.Should().BeEquivalentTo(new Size("The ".Length + "brown ".Length, 1));
        }
    }
}
