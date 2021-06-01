﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
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
            measuredSize.Height.Should().Be(0);
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Column_width_definition_size_calculation_according_priority_fixed_then_size_to_content_then_star(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Star(1), ColumnDefinition.SizeToContent(), ColumnDefinition.Fixed(10));
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped"), 2, 0);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 20, 1));

            terminal.Events.Should().BeEquivalentSequenceTo(
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                                                            new TestTerminal.ContentWritten("brown fox "),
                                                            new TestTerminal.CursorPositionChanged(new Point(10, 0)),
                                                            new TestTerminal.ContentWritten("jumped    "));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Row_height_definition_size_calculation_according_priority_fixed_then_size_to_content_then_star(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetRows(RowDefinition.Star(1), RowDefinition.SizeToContent(), RowDefinition.Fixed(4));
            grid.SetChild(new ContentView("The"), 0, 0);
            grid.SetChild(new ContentView("quick brown fox"), 0, 1);
            grid.SetChild(new ContentView("jumped over the sleepy"), 0, 2);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 8, 6));

            terminal.Events.Should().BeEquivalentSequenceTo(
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                                                            new TestTerminal.ContentWritten("quick   "),
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                                                            new TestTerminal.ContentWritten("brown   "),
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 2)),
                                                            new TestTerminal.ContentWritten("jumped  "),
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 3)),
                                                            new TestTerminal.ContentWritten("over the"),
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 4)),
                                                            new TestTerminal.ContentWritten("sleepy  "),
                                                            new TestTerminal.CursorPositionChanged(new Point(0, 5)),
                                                            new TestTerminal.ContentWritten("        "));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Column_width_definition_is_preserved_even_defintion_is_mixed_for_subsequent_columns(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Fixed(10), ColumnDefinition.Star(1), ColumnDefinition.Fixed(10), ColumnDefinition.SizeToContent());
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped"), 2, 0);
            grid.SetChild(new ContentView("over the sleepy"), 3, 0);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 115, 1));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick "),
                new TestTerminal.CursorPositionChanged(new Point(10, 0)),
                new TestTerminal.ContentWritten("brown fox" + new string(' ', 71)),
                new TestTerminal.CursorPositionChanged(new Point(90, 0)),
                new TestTerminal.ContentWritten("jumped    "),
                new TestTerminal.CursorPositionChanged(new Point(100, 0)),
                new TestTerminal.ContentWritten("over the sleepy"));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Star_grid_lays_out_in_even_grid(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Star(1), ColumnDefinition.Star(1));
            grid.SetRows(RowDefinition.Star(1), RowDefinition.Star(1));
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped"), 0, 1);
            grid.SetChild(new ContentView("over"), 1, 1);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 10, 4));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The  "),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("quick"),
                new TestTerminal.CursorPositionChanged(new Point(5, 0)),
                new TestTerminal.ContentWritten("brown"),
                new TestTerminal.CursorPositionChanged(new Point(5, 1)),
                new TestTerminal.ContentWritten("fox  "),
                new TestTerminal.CursorPositionChanged(new Point(0, 2)),
                new TestTerminal.ContentWritten("jumpe"),
                new TestTerminal.CursorPositionChanged(new Point(0, 3)),
                new TestTerminal.ContentWritten("     "),
                new TestTerminal.CursorPositionChanged(new Point(5, 2)),
                new TestTerminal.ContentWritten("over "),
                new TestTerminal.CursorPositionChanged(new Point(5, 3)),
                new TestTerminal.ContentWritten("     "));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Fixed_grid_lays_out_fixed_rows_and_columns(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.Fixed(6), ColumnDefinition.Fixed(4));
            grid.SetRows(RowDefinition.Fixed(1), RowDefinition.Fixed(2));
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 10, 4));

            terminal.Events
                   .Should()
                   .BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The   "),
                new TestTerminal.CursorPositionChanged(new Point(6, 0)),
                new TestTerminal.ContentWritten("brow"),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("jumped"),
                new TestTerminal.CursorPositionChanged(new Point(0, 2)),
                new TestTerminal.ContentWritten("over  "),
                new TestTerminal.CursorPositionChanged(new Point(6, 1)),
                new TestTerminal.ContentWritten("the "),
                new TestTerminal.CursorPositionChanged(new Point(6, 2)),
                new TestTerminal.ContentWritten("slee"));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Size_to_content_grid_with_wide_region_adjusts_to_content_size(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.SizeToContent(), ColumnDefinition.SizeToContent());
            grid.SetRows(RowDefinition.SizeToContent(), RowDefinition.SizeToContent());
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 25, 3));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick    "),
                new TestTerminal.CursorPositionChanged(new Point(13, 0)),
                new TestTerminal.ContentWritten("brown fox "),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("jumped over  "),
                new TestTerminal.CursorPositionChanged(new Point(13, 1)),
                new TestTerminal.ContentWritten("the sleepy"));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Size_to_content_grid_with_narrow_region_increases_row_height(OutputMode outputMode)
        {
            var grid = new GridView();
            grid.SetColumns(ColumnDefinition.SizeToContent(), ColumnDefinition.SizeToContent());
            grid.SetRows(RowDefinition.SizeToContent(), RowDefinition.SizeToContent());
            grid.SetChild(new ContentView("The quick"), 0, 0);
            grid.SetChild(new ContentView("brown fox"), 1, 0);
            grid.SetChild(new ContentView("jumped over"), 0, 1);
            grid.SetChild(new ContentView("the sleepy"), 1, 1);

            var terminal = new TestTerminal();
            var renderer = new ConsoleRenderer(terminal, outputMode);
            grid.Render(renderer, new Region(0, 0, 18, 3));

            terminal.Events.Should().BeEquivalentSequenceTo(
                new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                new TestTerminal.ContentWritten("The quick    "),
                new TestTerminal.CursorPositionChanged(new Point(0, 1)),
                new TestTerminal.ContentWritten("             "),
                new TestTerminal.CursorPositionChanged(new Point(13, 0)),
                new TestTerminal.ContentWritten("brown"),
                new TestTerminal.CursorPositionChanged(new Point(13, 1)),
                new TestTerminal.ContentWritten("fox  "),
                new TestTerminal.CursorPositionChanged(new Point(0, 2)),
                new TestTerminal.ContentWritten("jumped over  "),
                new TestTerminal.CursorPositionChanged(new Point(13, 2)),
                new TestTerminal.ContentWritten("the s"));
        }
    }
}
