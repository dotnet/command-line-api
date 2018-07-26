using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class SpanFormatterTests
    {
        private readonly ITestOutputHelper output;

        public SpanFormatterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void A_simple_formattable_string_can_be_converted_to_a_ContentSpan()
        {
            var span = new SpanFormatter().ParseToSpan($"some text");

            span.Should()
                .BeOfType<ContentSpan>()
                .Which
                .Content
                .Should()
                .Be("some text");
        }

        [Fact]
        public void A_formattable_string_containing_ansi_codes_can_be_converted_to_a_ContainerSpan()
        {
            var span = new SpanFormatter().ParseToSpan($"some {Ansi.Text.BlinkOn}blinking{Ansi.Text.BlinkOff} text");

            var containerSpan = span.Should()
                                    .BeOfType<ContainerSpan>()
                                    .Subject;

            containerSpan
                .Should()
                .BeEquivalentTo(
                    new Span[] {
                        new ContentSpan("some "),
                        Ansi.Text.BlinkOn,
                        new ContentSpan("blinking"),
                        Ansi.Text.BlinkOff,
                        new ContentSpan(" text")
                    },
                    options => options.WithStrictOrdering()
                );
        }

        [Theory]
        [MemberData(nameof(FormattableStringsWithEscapes))]
        public void FormattableString_parsing_handles_escapes(
            FormattableString fs,
            int expectedCount)
        {
            var formatter = new SpanFormatter();

            var span = formatter.ParseToSpan(fs);

            if (expectedCount > 1)
            {
                var containerSpan = span.Should()
                                        .BeOfType<ContainerSpan>()
                                        .Subject;

                output.WriteLine(containerSpan.ToString());

                containerSpan.Count.Should().Be(expectedCount);
            }
            else
            {
                span.Should().BeOfType<ContentSpan>();
            }
        }

        public static IEnumerable<object[]> FormattableStringsWithEscapes()
        {
            yield return Create($"{{", 1);
            yield return Create($"}}", 1);
            yield return Create($"{{{Ansi.Cursor.SavePosition}", 2);
            yield return Create($"{{{Ansi.Cursor.SavePosition}}}", 3);

            object[] Create(FormattableString fs, int expectedCount) =>
                new object[] {
                    fs,
                    expectedCount
                };
        }

        [Theory]
        [MemberData(nameof(FormattableStringsWithFormatStrings))]
        public void FormattableString_parsing_handles_format_strings(
            FormattableString fs,
            int expectedCount)
        {
            var formatter = new SpanFormatter();

            var span = formatter.ParseToSpan(fs);

            if (expectedCount > 1)
            {
                var containerSpan = span.Should()
                                        .BeOfType<ContainerSpan>()
                                        .Subject;

                output.WriteLine(containerSpan.ToString());

                containerSpan.Count.Should().Be(expectedCount);
            }
            else
            {
                span.Should().BeOfType<ContentSpan>();
            }
        }

        public static IEnumerable<object[]> FormattableStringsWithFormatStrings()
        {
            var date = new DateTime();

            yield return Create($"{{ {date:hh:mm:ss}", 2);
            yield return Create($"some text and a {date:s}", 2);

            object[] Create(FormattableString fs, int expectedCount) =>
                new object[] {
                    fs,
                    expectedCount
                };
        }
    }
}
