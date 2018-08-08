using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;

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
                    expectOutput: $"{Cursor.Move.ToLocation(1, 1)}The" +
                                  $"{Cursor.Move.ToLocation(2, 1)}qui" +
                                  $"{Cursor.Move.ToLocation(3, 1)}bro" +
                                  $"{Cursor.Move.ToLocation(4, 1)}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(13, 13)}The" +
                                  $"{Cursor.Move.ToLocation(14, 13)}qui" +
                                  $"{Cursor.Move.ToLocation(15, 13)}bro" +
                                  $"{Cursor.Move.ToLocation(16, 13)}fox");

                testCaseName = $"{nameof(AnsiControlCode)} at start of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{Color.Foreground.Red}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(1, 1)}{Color.Foreground.Red}The" +
                                  $"{Cursor.Move.ToLocation(2, 1)}qui" +
                                  $"{Cursor.Move.ToLocation(3, 1)}bro" +
                                  $"{Cursor.Move.ToLocation(4, 1)}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{Color.Foreground.Red}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(13, 13)}{Color.Foreground.Red}The" +
                                  $"{Cursor.Move.ToLocation(14, 13)}qui" +
                                  $"{Cursor.Move.ToLocation(15, 13)}bro" +
                                  $"{Cursor.Move.ToLocation(16, 13)}fox");

                testCaseName = $"{nameof(AnsiControlCode)} at end of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{Color.Foreground.Default}",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(1, 1)}The" +
                                  $"{Cursor.Move.ToLocation(2, 1)}qui" +
                                  $"{Cursor.Move.ToLocation(3, 1)}bro" +
                                  $"{Cursor.Move.ToLocation(4, 1)}fox{Color.Foreground.Default}");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{Color.Foreground.Default}",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(13, 13)}The" +
                                  $"{Cursor.Move.ToLocation(14, 13)}qui" +
                                  $"{Cursor.Move.ToLocation(15, 13)}bro" +
                                  $"{Cursor.Move.ToLocation(16, 13)}fox{Color.Foreground.Default}");

                testCaseName = $"{nameof(AnsiControlCode)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(1, 1)}The" +
                                  $"{Cursor.Move.ToLocation(2, 1)}qui{Color.Foreground.Rgb(139, 69, 19)}" +
                                  $"{Cursor.Move.ToLocation(3, 1)}bro{Color.Foreground.Default}" +
                                  $"{Cursor.Move.ToLocation(4, 1)}fox");

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    expectOutput: $"{Cursor.Move.ToLocation(13, 13)}The" +
                                  $"{Cursor.Move.ToLocation(14, 13)}qui{Color.Foreground.Rgb(139, 69, 19)}" +
                                  $"{Cursor.Move.ToLocation(15, 13)}bro{Color.Foreground.Default}" +
                                  $"{Cursor.Move.ToLocation(16, 13)}fox");

                testCaseName = $"{nameof(AnsiControlCode)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput: $"{Cursor.Move.ToLocation(1, 1)}The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.");
         
                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput: $"{Cursor.Move.ToLocation(13, 13)}The quick {Color.Foreground.Rgb(139, 69, 19)}brown{Color.Foreground.Default} fox jumps over the lazy dog.");
            }
        }
    }
}
