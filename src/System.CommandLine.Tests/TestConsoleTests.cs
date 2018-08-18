using System.CommandLine.Rendering;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class TestConsoleTests : ConsoleTests
    {
        protected override IConsole GetConsole() => new TestConsole();

        [Fact]
        public void When_CurorLeft_is_set_then_a_cursor_position_is_recorded()
        {
            var console = new TestConsole();

            console.CursorLeft = 19;

            console.Events
                   .OfType<TestConsole.CursorPositionChanged>()
                   .Select(e => e.Point)
                   .Should()
                   .BeEquivalentTo(new Point(19, 0));
        }

        [Fact]
        public void When_CursorTop_is_set_then_a_cursor_position_is_recorded()
        {
            var console = new TestConsole();

            console.CursorTop = 12;

            console.Events
                   .OfType<TestConsole.CursorPositionChanged>()
                   .Select(e => e.Point)
                   .Should()
                   .BeEquivalentTo(new Point(0, 12));
        }

        [Fact]
        public void When_SetCursorLocation_is_called_then_a_single_cursor_position_is_recorded()
        {
            var console = new TestConsole();

            console.SetCursorPosition(3, 5);

            console.Events
                   .OfType<TestConsole.CursorPositionChanged>()
                   .Select(e => e.Point)
                   .Should()
                   .BeEquivalentTo(new Point(3, 5));
        }

        [Theory]
        [InlineData(OutputMode.Ansi, "\n\n", Skip = "WIP")]
        [InlineData(OutputMode.Ansi, "\r\n\r\n", Skip = "WIP")]
        [InlineData(OutputMode.Ansi, "one\ntwo\nthree", Skip = "WIP")]
        [InlineData(OutputMode.Ansi, "one\r\ntwo\r\nthree", Skip = "WIP")]
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
                   .OfType<TestConsole.CursorPositionChanged>()
                   .Select(e => e.Point)
                   .Should()
                   .BeEquivalentTo(new[] {
                       new Point(2, 5),
                       new Point(2, 6),
                       new Point(2, 7),
                   });
        }

        [Fact]
        public void Timeline_allows_replay_of_render()
        {
            var console = new TestConsole();

            var renderer = new ConsoleRenderer(console, OutputMode.NonAnsi);

            var region = new Region(1, 3, 11, 2);

            renderer.RenderToRegion("first line\nsecond line", region);

            console.Events
                   .Should()
                   .BeEquivalentTo(new object[] {
                       new TestConsole.CursorPositionChanged(new Point(1, 3)),
                       new TestConsole.TextWritten("first line "),
                       new TestConsole.CursorPositionChanged(new Point(1, 4)),
                       new TestConsole.TextWritten("second line"),
                   }, options => options.WithStrictOrdering());
        }
    }
}
