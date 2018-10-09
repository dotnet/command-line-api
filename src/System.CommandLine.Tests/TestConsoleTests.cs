using System.CommandLine.Rendering;
using System.CommandLine.Tests.Rendering;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;
using static System.CommandLine.Tests.TestConsole;

namespace System.CommandLine.Tests
{
    public class TestConsoleTests : ConsoleTests
    {
        protected override IConsole GetConsole() => new TestConsole();

        [Fact]
        public void When_CursorLeft_is_set_then_a_cursor_position_is_recorded()
        {
            var console = new TestConsole();

            console.CursorLeft = 19;

            console.Events
                   .OfType<CursorPositionChanged>()
                   .Select(e => e.Position)
                   .Should()
                   .BeEquivalentSequenceTo(new Point(19, 0));
        }

        [Fact]
        public void When_CursorTop_is_set_then_a_cursor_position_is_recorded()
        {
            var console = new TestConsole();

            console.CursorTop = 12;

            console.Events
                   .OfType<CursorPositionChanged>()
                   .Select(e => e.Position)
                   .Should()
                   .BeEquivalentSequenceTo(new Point(0, 12));
        }

        [Fact]
        public void When_SetCursorLocation_is_called_then_a_single_cursor_position_is_recorded()
        {
        }

        [Fact]
        public void When_ANSI_sequences_are_used_to_set_cursor_positions_then_CursorPositionChanged_events_are_recorded()
        {
            var console = new TestConsole();

            console.Out.Write($"before move{Ansi.Cursor.Move.ToLocation(3, 5).EscapeSequence}after move");

            console.Events
                   .Should()
                   .BeEquivalentSequenceTo(
                       new ContentWritten("before move"),
                       new CursorPositionChanged(new Point(2, 4)),
                       new ContentWritten("after move"));
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
            var console = new TestConsole();

            var renderer = new ConsoleRenderer(console, outputMode);

            renderer.RenderToRegion(threeLinesOfText, new Region(2, 5, 13, 3));

            console.Events
                   .OfType<CursorPositionChanged>()
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
            var console = new TestConsole();

            var renderer = new ConsoleRenderer(console, outputMode);

            var region = new Region(1, 3, 11, 2);

            renderer.RenderToRegion("first line\nsecond line", region);

            console.Events
                   .Where(e => !(e is AnsiControlCodeWritten))
                   .Should()
                   .BeEquivalentSequenceTo(
                       new CursorPositionChanged(new Point(1, 3)),
                       new ContentWritten("first line "),
                       new CursorPositionChanged(new Point(1, 4)),
                       new ContentWritten("second line"));
        }

        [Fact]
        public void ContentWritten_events_do_not_include_escape_sequences()
        {
            var console = new TestConsole();

            var renderer = new ConsoleRenderer(console, OutputMode.Ansi);

            var region = new Region(0, 0, 4, 1);

            renderer.RenderToRegion($"{ForegroundColorSpan.Red()}text{ForegroundColorSpan.Reset()}", region);

            console.Events
                   .Should()
                   .Contain(e => e is ContentWritten &&
                                 ((ContentWritten)e).Content == "text");
        }
    }
}
