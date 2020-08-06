// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class StackLayoutViewTests
    {
        [Fact]
        public void Vertical_stack_displays_content_stacked_on_top_of_each_other()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 10, 2));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick"),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("brown fox"));
        }

        [Fact]
        public void Vertical_stack_clips_content_when_region_is_not_tall_enough()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 10, 1));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick"));
        }

        [Fact]
        public void Vertical_stack_wraps_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView();
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 5, 4));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The  "),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("quick"),
                new TestTerminal.CursorPositionChanged(new Point(0, 2)),
                new TestTerminal.ContentWritten("brown"),
                new TestTerminal.CursorPositionChanged(new Point(0, 3)),
                new TestTerminal.ContentWritten("fox  ")
                );
        }

        [Fact]
        public void Horizontal_stack_displays_content_stacked_on_next_to_each_other()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 18, 1));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick         "),
                new TestTerminal.CursorPositionChanged(new Point(9, 0)),
                new TestTerminal.ContentWritten("brown fox"));
        }

        [Fact]
        public void Horizontal_stack_clips_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 16, 1));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick       "),
                new TestTerminal.CursorPositionChanged(new Point(9, 0)),
                new TestTerminal.ContentWritten("brown  "));
        }

        [Fact]
        public void Horizontal_stack_wraps_content_when_region_is_not_wide_enough()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            stackLayout.Render(renderer, new Region(0, 0, 14, 2));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick     "),
                new TestTerminal.CursorPositionChanged(new Point(9, 0)),
                new TestTerminal.ContentWritten("brown"),
                new TestTerminal.CursorPositionChanged(new Point(9, 1)),
                new TestTerminal.ContentWritten("fox  ")
            );
        }

        [Fact]
        public void Measuring_a_vertical_stack_sums_content_height()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
            var size = stackLayout.Measure(renderer, new Size(10, 10));

            size.Should().BeEquivalentTo(new Size(9, 2));
        }

        [Fact]
        public void Measuring_a_vertical_stack_with_word_wrap_it_sums_max_height_for_each_row()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
            var size = stackLayout.Measure(renderer, new Size(7, 10));

            size.Should().BeEquivalentTo(new Size("brown ".Length, 4));
        }

        [Fact]
        public void Measuring_a_vertical_stack_with_row_truncation_the_top_row_is_measured_first()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
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

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
            var size = stackLayout.Measure(renderer, new Size(20, 20));

            size.Should().BeEquivalentTo(new Size("The quickbrown fox".Length, 1));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_with_word_wrap_it_sums_max_width_for_each_child()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
            var size = stackLayout.Measure(renderer, new Size(10, 10));
            
            size.Should().BeEquivalentTo(new Size(10, 2));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_with_truncated_height_measures_max_for_each_child()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);
            
            var size = stackLayout.Measure(renderer, new Size(7, 1));

            size.Should().BeEquivalentTo(new Size(7, 1));
        }

        [Fact]
        public void Measuring_a_horizontal_stack_with_wide_children_wraps_last_child()
        {
            var stackLayout = new StackLayoutView(Orientation.Horizontal);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);

            var size = stackLayout.Measure(renderer, new Size(12, 5));

            size.Should().BeEquivalentTo(new Size(12, 2));
        }

        [Fact]
        public void Measuring_a_vertical_stack_with_tall_children_trims_last_child()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);
            var child1 = new ContentView("The quick");
            var child2 = new ContentView("brown fox");

            stackLayout.Add(child1);
            stackLayout.Add(child2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal);

            var size = stackLayout.Measure(renderer, new Size(5, 3));

            size.Should().BeEquivalentTo(new Size(5, 3));
        }
    }
}
