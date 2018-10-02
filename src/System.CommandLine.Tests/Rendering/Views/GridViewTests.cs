using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Drawing;
using FluentAssertions;
using Xunit;
using Size = System.CommandLine.Rendering.Size;

namespace System.CommandLine.Tests.Rendering.Views
{
    public class GridViewTests
    {
        [Fact]
        public void Empty_star_sized_grid_fills_entire_region()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Star(1), ColumnDefinition.Star(1));
            grid.SetRows(RowDefinition.Star(1), RowDefinition.Star(1));

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            Size measuredSize = grid.Measure(renderer, new Size(10, 10));

            measuredSize.Width.Should().Be(10);
            measuredSize.Height.Should().Be(10);
        }

        [Fact]
        public void Empty_fixed_sized_grid_returns_fixed_region()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Fixed(7), ColumnDefinition.Star(3));
            grid.SetRows(RowDefinition.Fixed(3), RowDefinition.Star(7));

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            Size measuredSize = grid.Measure(renderer, new Size(10, 10));

            measuredSize.Width.Should().Be(10);
            measuredSize.Height.Should().Be(10);
        }

        [Fact]
        public void Empty_size_to_content_grid_do_not_take_up_space()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.SizeToContent(), ColumnDefinition.SizeToContent());
            grid.SetRows(RowDefinition.SizeToContent(), RowDefinition.SizeToContent());

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            Size measuredSize = grid.Measure(renderer, new Size(10, 10));

            measuredSize.Width.Should().Be(0);
            measuredSize.Height.Should().Be(
0);
        }

        [Fact]
        public void Star_grid_lays_out_in_even_grid()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Star(1), ColumnDefinition.Star(1));
            grid.SetRows(RowDefinition.Star(1), RowDefinition.Star(1));
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped"), 0, 1);
            grid.SetChild(new ContentView("over"), 1, 1);


            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            grid.Render(renderer, new Region(0, 0, 10, 4));

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The  "),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("quick"),
                new TestConsole.CursorPositionChanged(new Point(5, 0)),
                new TestConsole.ContentWritten("brown"),
                new TestConsole.CursorPositionChanged(new Point(5, 1)),
                new TestConsole.ContentWritten("fox  "),
                new TestConsole.CursorPositionChanged(new Point(0, 2)),
                new TestConsole.ContentWritten("jumpe"),
                new TestConsole.CursorPositionChanged(new Point(0, 3)),
                new TestConsole.ContentWritten("     "),
                new TestConsole.CursorPositionChanged(new Point(5, 2)),
                new TestConsole.ContentWritten("over "),
                new TestConsole.CursorPositionChanged(new Point(5, 3)),
                new TestConsole.ContentWritten("     "));
        }

        [Fact]
        public void Fixed_grid_lays_out_fixed_rows_and_columns()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Fixed(6), ColumnDefinition.Fixed(4));
            grid.SetRows(RowDefinition.Fixed(1), RowDefinition.Fixed(2));
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            grid.Render(renderer, new Region(0, 0, 10, 4));

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The   "),
                new TestConsole.CursorPositionChanged(new Point(6, 0)),
                new TestConsole.ContentWritten("brow"),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("jumped"),
                new TestConsole.CursorPositionChanged(new Point(0, 2)),
                new TestConsole.ContentWritten("over  "),
                new TestConsole.CursorPositionChanged(new Point(6, 1)),
                new TestConsole.ContentWritten("the "),
                new TestConsole.CursorPositionChanged(new Point(6, 2)),
                new TestConsole.ContentWritten("slee"));
        }

        [Fact]
        public void Size_to_content_grid_with_wide_region_adjusts_to_content_size()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.SizeToContent(), ColumnDefinition.SizeToContent());
            grid.SetRows(RowDefinition.SizeToContent(), RowDefinition.SizeToContent());
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            grid.Render(renderer, new Region(0, 0, 25, 3));

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick  "),
                new TestConsole.CursorPositionChanged(new Point(11, 0)),
                new TestConsole.ContentWritten("brown fox "),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("jumped over"),
                new TestConsole.CursorPositionChanged(new Point(11, 1)),
                new TestConsole.ContentWritten("the sleepy"));
        }

        [Fact]
        public void Size_to_content_grid_with_narrow_region_increases_row_height()
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.SizeToContent(), ColumnDefinition.SizeToContent());
            grid.SetRows(RowDefinition.SizeToContent(), RowDefinition.SizeToContent());
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var console = new TestConsole();
            var renderer = new ConsoleRenderer(console);
            grid.Render(renderer, new Region(0, 0, 18, 3));

            console.Events.Should().BeEquivalentTo(
                new TestConsole.CursorPositionChanged(new Point(0, 0)),
                new TestConsole.ContentWritten("The quick  "),
                new TestConsole.CursorPositionChanged(new Point(0, 1)),
                new TestConsole.ContentWritten("           "),
                new TestConsole.CursorPositionChanged(new Point(11, 0)),
                new TestConsole.ContentWritten("brown "),
                new TestConsole.CursorPositionChanged(new Point(11, 1)),
                new TestConsole.ContentWritten("fox   "),
                new TestConsole.CursorPositionChanged(new Point(0, 2)),
                new TestConsole.ContentWritten("jumped over"),
                new TestConsole.CursorPositionChanged(new Point(11, 2)),
                new TestConsole.ContentWritten("the   "));
        }
    }
}
