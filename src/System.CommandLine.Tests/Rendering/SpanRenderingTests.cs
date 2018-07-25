using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class SpanRenderingTests
    {
        private readonly ITestOutputHelper output;
        private readonly TestConsole _console;

        public SpanRenderingTests(ITestOutputHelper output)
        {
            this.output = output;
            _console = new TestConsole();
        }

        [Theory]
        [MemberData(nameof(QuickBrownFoxVariants))]
        public void In_content_only_mode_wrap_does_not_wrap_span_to_more_lines_than_specified(Span span)
        {
            _console.Height = 4;
            _console.Width = 6;

            new ConsoleWriter(_console)
                .RenderToRegion(
                    span,
                    _console.GetRegion());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"The   {NewLine}" +
                        $"quick {NewLine}" +
                        $"brown {NewLine}" +
                        $"fox   ");
        }

        [Theory]
        [MemberData(nameof(QuickBrownFoxVariants))]
        public void In_content_only_mode_wrap_chops_words_that_are_wider_than_the_region(Span span)
        {
            _console.Height = 4;
            _console.Width = 3;

            new ConsoleWriter(_console)
                .RenderToRegion(
                    span,
                    _console.GetRegion());

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"The{NewLine}" +
                        $"qui{NewLine}" +
                        $"bro{NewLine}" +
                        $"fox");
        }

        public static IEnumerable<object[]> QuickBrownFoxVariants()
        {
            var spans =
                new[] {
                    new ContentSpan("The quick brown fox jumps over the lazy dog.")
                };

            foreach (var span in spans)
            {
                yield return new object[] { span };
            }
        }
    }
}
