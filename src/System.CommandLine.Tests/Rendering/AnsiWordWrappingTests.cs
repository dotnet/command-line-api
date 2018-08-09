using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;
using static System.CommandLine.Rendering.Ansi.Cursor;

namespace System.CommandLine.Tests.Rendering
{
    public class AnsiWordWrappingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public AnsiWordWrappingTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void In_ansi_mode_word_wrap_wraps_correctly(RenderingTestCase @case)
        {
            new ConsoleRenderer(
                    _console,
                    OutputMode.Ansi)
                .RenderToRegion(
                    @case.InputSpan,
                    @case.Region);

            _output.WriteLine(_console.Out.ToString());

            _console.Out
                    .ToString()
                    .Should()
                    .Be(@case.ExpectedOutput);
        }

        public static IEnumerable<object[]> TestCases()
        {
            return TestCases().Select(t => new object[] { t }).ToArray();

            IEnumerable<RenderingTestCase> TestCases()
            {
                var testCaseName = $"{nameof(ContentSpan)} only";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Move.ToLocation(1, 1).EscapeSequence}The" +
                                  $"{Move.ToLocation(2, 1).EscapeSequence}qui" +
                                  $"{Move.ToLocation(3, 1).EscapeSequence}bro" +
                                  $"{Move.ToLocation(4, 1).EscapeSequence}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Move.ToLocation(13, 13).EscapeSequence}The" +
                                  $"{Move.ToLocation(14, 13).EscapeSequence}qui" +
                                  $"{Move.ToLocation(15, 13).EscapeSequence}bro" +
                                  $"{Move.ToLocation(16, 13).EscapeSequence}fox");

                testCaseName = $"{nameof(FormatSpan)} at start of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{ForegroundColorSpan.Red}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Move.ToLocation(1, 1).EscapeSequence}{Color.Foreground.Red.EscapeSequence}The" +
                                  $"{Move.ToLocation(2, 1).EscapeSequence}qui" +
                                  $"{Move.ToLocation(3, 1).EscapeSequence}bro" +
                                  $"{Move.ToLocation(4, 1).EscapeSequence}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{ForegroundColorSpan.Red}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Move.ToLocation(13, 13).EscapeSequence}{Color.Foreground.Red.EscapeSequence}The" +
                                  $"{Move.ToLocation(14, 13).EscapeSequence}qui" +
                                  $"{Move.ToLocation(15, 13).EscapeSequence}bro" +
                                  $"{Move.ToLocation(16, 13).EscapeSequence}fox");

                testCaseName = $"{nameof(FormatSpan)} at end of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{ForegroundColorSpan.Reset}",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Move.ToLocation(1, 1).EscapeSequence}The" +
                                  $"{Move.ToLocation(2, 1).EscapeSequence}qui" +
                                  $"{Move.ToLocation(3, 1).EscapeSequence}bro" +
                                  $"{Move.ToLocation(4, 1).EscapeSequence}fox{Color.Foreground.Default.EscapeSequence}");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{ForegroundColorSpan.Reset}",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Move.ToLocation(13, 13).EscapeSequence}The" +
                                  $"{Move.ToLocation(14, 13).EscapeSequence}qui" +
                                  $"{Move.ToLocation(15, 13).EscapeSequence}bro" +
                                  $"{Move.ToLocation(16, 13).EscapeSequence}fox{Color.Foreground.Default.EscapeSequence}");

                testCaseName = $"{nameof(FormatSpan)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Move.ToLocation(1, 1).EscapeSequence}The" +
                                  $"{Move.ToLocation(2, 1).EscapeSequence}qui{Color.Foreground.Rgb(139, 69, 19).EscapeSequence}" +
                                  $"{Move.ToLocation(3, 1).EscapeSequence}bro{Color.Foreground.Default.EscapeSequence}" +
                                  $"{Move.ToLocation(4, 1).EscapeSequence}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Move.ToLocation(13, 13).EscapeSequence}The" +
                                  $"{Move.ToLocation(14, 13).EscapeSequence}qui{Color.Foreground.Rgb(139, 69, 19).EscapeSequence}" +
                                  $"{Move.ToLocation(15, 13).EscapeSequence}bro{Color.Foreground.Default.EscapeSequence}" +
                                  $"{Move.ToLocation(16, 13).EscapeSequence}fox");

                testCaseName = $"{nameof(FormatSpan)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput: $"{Move.ToLocation(1, 1).EscapeSequence}The quick {Color.Foreground.Rgb(139, 69, 19).EscapeSequence}brown{Color.Foreground.Default.EscapeSequence} fox jumps over the lazy dog.");
         
                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput: $"{Move.ToLocation(13, 13).EscapeSequence}The quick {Color.Foreground.Rgb(139, 69, 19).EscapeSequence}brown{Color.Foreground.Default.EscapeSequence} fox jumps over the lazy dog.");
            }
        }
    }
}
