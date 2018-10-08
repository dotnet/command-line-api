using System.CommandLine.Rendering;
using System.Drawing;
using FluentAssertions;
using Xunit;
using static System.CommandLine.Tests.TestConsole;

namespace System.CommandLine.Tests.Rendering
{
    public class TextStyleRenderingTests
    {
        private readonly SpanFormatter _spanFormatter = new SpanFormatter();
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public void BackgroundColorSpans_are_replaced_with_ANSI_codes_during_ANSI_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{BackgroundColorSpan.Red}red {BackgroundColorSpan.Blue}blue {BackgroundColorSpan.Green}green {BackgroundColorSpan.Reset}or a {BackgroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.Ansi);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Out
                    .ToString()
                    .Should()
                    .Contain(
                        $"{Ansi.Color.Background.Red.EscapeSequence}red {Ansi.Color.Background.Blue.EscapeSequence}blue {Ansi.Color.Background.Green.EscapeSequence}green {Ansi.Color.Background.Default.EscapeSequence}or a {Ansi.Color.Background.Rgb(12, 34, 56).EscapeSequence}little of each.");
        }

        [Fact]
        public void ForegroundColorSpans_are_replaced_with_ANSI_codes_during_ANSI_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{ForegroundColorSpan.Red}red {ForegroundColorSpan.Blue}blue {ForegroundColorSpan.Green}green {ForegroundColorSpan.Reset}or a {ForegroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.Ansi);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Out
                    .ToString()
                    .Should()
                    .Contain(
                        $"{Ansi.Color.Foreground.Red.EscapeSequence}red {Ansi.Color.Foreground.Blue.EscapeSequence}blue {Ansi.Color.Foreground.Green.EscapeSequence}green {Ansi.Color.Foreground.Default.EscapeSequence}or a {Ansi.Color.Foreground.Rgb(12, 34, 56).EscapeSequence}little of each.");
        }

        [Fact]
        public void BackgroundColorSpans_are_replaced_with_System_Console_calls_during_non_ANSI_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{BackgroundColorSpan.Red}red {BackgroundColorSpan.Blue}blue {BackgroundColorSpan.Green}green {BackgroundColorSpan.Reset}or a {BackgroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.NonAnsi);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new CursorPositionChanged(new Point(0, 0)),
                        new BackgroundColorChanged(ConsoleColor.DarkRed),
                        new ContentWritten("red "),
                        new BackgroundColorChanged(ConsoleColor.DarkBlue),
                        new ContentWritten("blue "),
                        new BackgroundColorChanged(ConsoleColor.DarkGreen),
                        new ContentWritten("green "),
                        new ColorReset(),
                        new ForegroundColorChanged(ConsoleColor.White),
                        new ContentWritten("or a "),
                        new ColorReset(),
                        new ForegroundColorChanged(ConsoleColor.White),
                        new ContentWritten("little of each.")
                    );
        }

        [Fact]
        public void ForegroundColorSpans_are_replaced_with_System_Console_calls_during_non_ANSI_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{ForegroundColorSpan.Red}red {ForegroundColorSpan.Blue}blue {ForegroundColorSpan.Green}green {ForegroundColorSpan.Reset}or a {ForegroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.NonAnsi);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new CursorPositionChanged(new Point(0, 0)),
                        new ForegroundColorChanged(ConsoleColor.DarkRed),
                        new ContentWritten("red "),
                        new ForegroundColorChanged(ConsoleColor.DarkBlue),
                        new ContentWritten("blue "),
                        new ForegroundColorChanged(ConsoleColor.DarkGreen),
                        new ContentWritten("green "),
                        new ColorReset(),
                        new BackgroundColorChanged(ConsoleColor.Black),
                        new ContentWritten("or a "),
                        new ColorReset(),
                        new BackgroundColorChanged(ConsoleColor.Black),
                        new ContentWritten("little of each.")
                    );
        }

        [Fact]
        public void BackgroundColorSpans_are_removed_during_file_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{BackgroundColorSpan.Red}red {BackgroundColorSpan.Blue}blue {BackgroundColorSpan.Green}green {BackgroundColorSpan.Reset}or a {BackgroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.File);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Out.ToString().Should().Be("red blue green or a little of each.");
        }

        [Fact]
        public void ForegroundColorSpans_are_removed_during_file_rendering()
        {
            var span = _spanFormatter.ParseToSpan(
                $"{ForegroundColorSpan.Red}red {ForegroundColorSpan.Blue}blue {ForegroundColorSpan.Green}green {ForegroundColorSpan.Reset}or a {ForegroundColorSpan.Rgb(12, 34, 56)}little of each.");

            var renderer = new ConsoleRenderer(_console, OutputMode.File);

            renderer.RenderToRegion(span, new Region(0, 0, 200, 1, false));

            _console.Out.ToString().Should().Be("red blue green or a little of each.");
        }
    }
}
