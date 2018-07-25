using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class SpanTests
    {
        [Fact]
        public void Content_span_length_is_the_same_as_the_contained_string_length()
        {
            new ContentSpan("the content")
                .ContentLength
                .Should()
                .Be("the content".Length);
        }

        [Fact]
        public void When_spans_are_nested_then_content_length_can_be_calculated()
        {
            var span = new ContainerSpan(
                Ansi.Color.Foreground.Red,
                new ContentSpan("content"),
                Ansi.Color.Foreground.Default);

            span.ContentLength.Should().Be("content".Length);
        }
    }
}
