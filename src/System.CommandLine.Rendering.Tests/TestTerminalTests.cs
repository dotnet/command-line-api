// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class TestTerminalTests : TerminalTests
    {
        protected TestTerminal _terminal = new();

        protected override ITerminal GetTerminal() => _terminal;

        [Fact]
        public void When_CursorLeft_is_set_then_a_cursor_position_is_recorded()
        {
            var terminal = (TestTerminal)GetTerminal();

            terminal.CursorLeft = 19;

            terminal.Events
                    .OfType<TestTerminal.CursorPositionChanged>()
                    .Select(e => e.Position)
                    .Should()
                    .BeEquivalentSequenceTo(new Point(19, 0));
        }

        [Fact]
        public void When_CursorTop_is_set_then_a_cursor_position_is_recorded()
        {
            var terminal = (TestTerminal)GetTerminal();

            terminal.CursorTop = 12;

            terminal.Events
                    .OfType<TestTerminal.CursorPositionChanged>()
                    .Select(e => e.Position)
                    .Should()
                    .BeEquivalentSequenceTo(new Point(0, 12));
        }

        [Fact]
        public void When_in_ANSI_mode_and_ANSI_sequences_are_used_to_set_cursor_positions_then_a_CursorPositionChanged_events_is_recorded()
        {
            var terminal = (TestTerminal)GetTerminal();

            terminal.IsAnsiTerminal = true;

            terminal.Out.Write($"before move{Ansi.Cursor.Move.ToLocation(3, 5).EscapeSequence}after move");

            terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new TestTerminal.ContentWritten("before move"),
                        new TestTerminal.CursorPositionChanged(new Point(2, 4)),
                        new TestTerminal.ContentWritten("after move"));
        }

        [Fact]
        public void When_not_in_ANSI_mode_and_ANSI_sequences_are_used_to_set_cursor_positions_then_a_CursorPositionChanged_events_is_recorded()
        {
            var terminal = (TestTerminal)GetTerminal();
            
            terminal.IsAnsiTerminal = false;

            var stringWithEscapeSequence = $"before move{Ansi.Cursor.Move.ToLocation(3, 5).EscapeSequence}after move";

            terminal.Out.Write(stringWithEscapeSequence);

            terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(new TestTerminal.ContentWritten(stringWithEscapeSequence));
        }

        [Theory]
        [InlineData(OutputMode.Ansi, "\n\n")]
        [InlineData(OutputMode.Ansi, "\r\n\r\n")]
        [InlineData(OutputMode.Ansi, "one\ntwo\nthree")]
        [InlineData(OutputMode.Ansi, "one\r\ntwo\r\nthree")]
        [InlineData(OutputMode.NonAnsi, "\n\n")]
        [InlineData(OutputMode.NonAnsi, "\r\n\r\n")]
        [InlineData(OutputMode.NonAnsi, "one\ntwo\nthree")]
        [InlineData(OutputMode.NonAnsi, "one\r\ntwo\r\nthree")]
        public void When_a_newline_is_written_by_a_ConsoleRenderer_then_a_cursor_position_is_recorded(
            OutputMode outputMode,
            string threeLinesOfText)
        {
            var terminal = (TestTerminal)GetTerminal();

            var renderer = new ConsoleRenderer(terminal, outputMode);

            renderer.RenderToRegion(threeLinesOfText, new Region(2, 5, 13, 3));

            terminal.Events
                   .OfType<TestTerminal.CursorPositionChanged>()
                   .Select(e => e.Position)
                   .Should()
                   .BeEquivalentSequenceTo(
                       new Point(2, 5),
                       new Point(2, 6),
                       new Point(2, 7));
        }

        [Theory]
        [InlineData(OutputMode.Ansi)]
        [InlineData(OutputMode.NonAnsi)]
        public void Timeline_allows_replay_of_content_rendering_and_cursor_positions(OutputMode outputMode)
        {
            var terminal = (TestTerminal)GetTerminal();

            var renderer = new ConsoleRenderer(terminal, outputMode);

            var region = new Region(1, 3, 11, 2);

            renderer.RenderToRegion("first line\nsecond line", region);

            terminal.Events
                    .Where(e => !(e is TestTerminal.AnsiControlCodeWritten))
                    .Should()
                    .BeEquivalentSequenceTo(
                        new TestTerminal.CursorPositionChanged(new Point(1, 3)),
                        new TestTerminal.ContentWritten("first line "),
                        new TestTerminal.CursorPositionChanged(new Point(1, 4)),
                        new TestTerminal.ContentWritten("second line"));
        }

        [Fact]
        public void When_in_ANSI_mode_then_ContentWritten_events_do_not_include_escape_sequences()
        {
            var terminal = (TestTerminal)GetTerminal();

            var renderer = new ConsoleRenderer(terminal, OutputMode.Ansi);

            var region = new Region(0, 0, 4, 1);

            renderer.RenderToRegion($"{ForegroundColorSpan.Red()}text{ForegroundColorSpan.Reset()}", region);

            terminal.Events
                    .Should()
                    .Contain(e => e is TestTerminal.ContentWritten &&
                                  ((TestTerminal.ContentWritten)e).Content == "text");
        }
    }
}
