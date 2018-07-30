using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class NonAnsiWordWrappingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public NonAnsiWordWrappingTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void In_non_ansi_mode_word_wrap_wraps_correctly(
            RenderingTestCase @case)
        {
            new ConsoleWriter(
                    _console,
                    OutputMode.NonAnsi)
                .RenderToRegion(
                    @case.InputSpan,
                    @case.Region);

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
                 yield return new RenderingTestCase(
                     name: $"{nameof(ContentSpan)} only",
                     rendering: $"The quick brown fox jumps over the lazy dog.",
                     inRegion: new Region(4, 3, 0, 0),
                     expectOutput: $"The{NewLine}" +
                                   $"qui{NewLine}" +
                                   $"bro{NewLine}" +
                                   $"fox");
                
                 yield return new RenderingTestCase(
                     name: $"{nameof(AnsiControlCode)} at start of {nameof(ContentSpan)}",
                     rendering: $"{Ansi.Clear.ToEndOfLine}The quick brown fox jumps over the lazy dog.",
                     inRegion: new Region(4, 3, 0, 0),
                     expectOutput: $"The{NewLine}" +
                                   $"qui{NewLine}" +
                                   $"bro{NewLine}" +
                                   $"fox");
                
                 yield return new RenderingTestCase(
                     name: $"{nameof(AnsiControlCode)} at end of {nameof(ContentSpan)}",
                     rendering: $"The quick brown fox jumps over the lazy dog.{Ansi.Clear.ToEndOfLine}",
                     inRegion: new Region(4, 3, 0, 0),
                     expectOutput: $"The{NewLine}" +
                                   $"qui{NewLine}" +
                                   $"bro{NewLine}" +
                                   $"fox");

                yield return new RenderingTestCase(
                    name: $"{nameof(AnsiControlCode)}s around a word inside a {nameof(ContentSpan)}",
                    rendering: $"The quick {Ansi.Color.Foreground.Rgb(139, 69, 19)}brown{Ansi.Color.Foreground.Default} fox jumps over the lazy dog.",
                    inRegion: new Region(4, 3, 0, 0),
                    expectOutput: $"The{NewLine}" +
                                  $"qui{NewLine}" +
                                  $"bro{NewLine}" +
                                  $"fox");
            }
        }
    }
}
