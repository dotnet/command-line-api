// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    // see https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
    public class AnsiTests
    {
        [Fact]
        public void Ansi_esc_returns_correct_terminal_sequence()
        {
            Ansi.Esc.Should().Be("\u001b");
        }

        [Fact]
        public void Ansi_cursor_move_up_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.Up().EscapeSequence.Should().Be($"{Ansi.Esc}[1A");
            Ansi.Cursor.Move.Up(5).EscapeSequence.Should().Be($"{Ansi.Esc}[5A");
        }

        [Fact]
        public void Ansi_cursor_move_down_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.Down().EscapeSequence.Should().Be($"{Ansi.Esc}[1B");
            Ansi.Cursor.Move.Down(5).EscapeSequence.Should().Be($"{Ansi.Esc}[5B");
        }

        [Fact]
        public void Ansi_cursor_move_right_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.Right().EscapeSequence.Should().Be($"{Ansi.Esc}[1C");
            Ansi.Cursor.Move.Right(5).EscapeSequence.Should().Be($"{Ansi.Esc}[5C");
        }

        [Fact]
        public void Ansi_cursor_move_left_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.Left().EscapeSequence.Should().Be($"{Ansi.Esc}[1D");
            Ansi.Cursor.Move.Left(5).EscapeSequence.Should().Be($"{Ansi.Esc}[5D");
        }

        [Fact]
        public void Ansi_cursor_move_next_line_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.NextLine().EscapeSequence.Should().Be($"{Ansi.Esc}[1E");
            Ansi.Cursor.Move.NextLine(5).EscapeSequence.Should().Be($"{Ansi.Esc}[5E");
        }

        [Fact]
        public void Ansi_cursor_move_to_upper_left_corner_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.ToUpperLeftCorner.EscapeSequence.Should().Be($"{Ansi.Esc}[H");
        }

        [Fact]
        public void Ansi_cursor_move_to_location_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Move.ToLocation().EscapeSequence.Should().Be($"{Ansi.Esc}[1;1H");
            Ansi.Cursor.Move.ToLocation(3).EscapeSequence.Should().Be($"{Ansi.Esc}[1;3H");
            Ansi.Cursor.Move.ToLocation(top: 2).EscapeSequence.Should().Be($"{Ansi.Esc}[2;1H");
            Ansi.Cursor.Move.ToLocation(5,4).EscapeSequence.Should().Be($"{Ansi.Esc}[4;5H");
        }

        [Fact]
        public void Ansi_cursor_scroll_up_one_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Scroll.UpOne.EscapeSequence.Should().Be($"{Ansi.Esc}[S");
        }

        [Fact]
        public void Ansi_cursor_scroll_down_one_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Scroll.DownOne.EscapeSequence.Should().Be($"{Ansi.Esc}[T");
        }

        [Fact]
        public void Ansi_cursor_hide_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Hide.EscapeSequence.Should().Be($"{Ansi.Esc}[?25l");
        }

        [Fact]
        public void Ansi_cursor_show_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.Show.EscapeSequence.Should().Be($"{Ansi.Esc}[?25h");
        }

        [Fact]
        public void Ansi_cursor_save_position_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.SavePosition.EscapeSequence.Should().Be($"{Ansi.Esc}7");
        }

        [Fact]
        public void Ansi_cursor_restore_position_returns_correct_terminal_sequence()
        {
            Ansi.Cursor.RestorePosition.EscapeSequence.Should().Be($"{Ansi.Esc}8");
        }
    }
}
