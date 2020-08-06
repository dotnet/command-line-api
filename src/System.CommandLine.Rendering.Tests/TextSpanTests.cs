// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class TextSpanTests
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
                ForegroundColorSpan.Red(),
                new ContentSpan("content"),
                ForegroundColorSpan.Reset());

            span.ContentLength.Should().Be("content".Length);
        }

        [Fact]
        public void When_a_span_has_no_parent_then_Root_returns_itself()
        {
            var span = new ContentSpan("some content");

            span.Root.Should().BeSameAs(span);
        }

        [Fact]
        public void Root_returns_the_outermost_parent_span()
        {
            var inner = new ContainerSpan();
            var root = new ContainerSpan(new ContainerSpan(new ContainerSpan(inner)));

            inner.Root.Should().BeSameAs(root);
        }

        [Fact]
        public void Spans_have_a_start_relative_to_the_parent_span()
        {
            var span = new ContainerSpan(
                ForegroundColorSpan.Red(),
                new ContentSpan("first"),
                ForegroundColorSpan.Blue(),
                new ContentSpan("second"),
                ForegroundColorSpan.Reset());

            span[0].Start.Should().Be(0);
            span[1].Start.Should().Be(0);
            span[2].Start.Should().Be("first".Length);
            span[3].Start.Should().Be("first".Length);
            span[4].Start.Should().Be("firstsecond".Length);
        }

        [Fact]
        public void Span_starts_update_when_parent_is_added_to_another_parent_span()
        {
            var innerContainerSpan = new ContainerSpan(
                ForegroundColorSpan.Red(),
                new ContentSpan("second"),
                ForegroundColorSpan.Blue(),
                new ContentSpan("third"),
                ForegroundColorSpan.Reset());

            var outerContainer = new ContainerSpan(
                new ContentSpan("first"),
                innerContainerSpan);

            innerContainerSpan[0].Start.Should().Be("first".Length);
            innerContainerSpan[1].Start.Should().Be("first".Length);
            innerContainerSpan[2].Start.Should().Be("firstsecond".Length);
            innerContainerSpan[3].Start.Should().Be("firstsecond".Length);
            innerContainerSpan[4].Start.Should().Be("firstsecondthird".Length);
        }

        [Fact]
        public void ToString_with_non_ansi_omits_ANSI_codes()
        {
            var span = new TextSpanFormatter()
                .ParseToSpan($"one{ForegroundColorSpan.Red()}two{ForegroundColorSpan.Reset()}three");

            span.ToString(OutputMode.NonAnsi)
                .Should()
                .Be("onetwothree");
        }

        [Fact]
        public void ToString_with_ansi_includes_ANSI_codes()
        {
            var span = new TextSpanFormatter()
                .ParseToSpan($"one{ForegroundColorSpan.Red()}two{ForegroundColorSpan.Reset()}three");

            span.ToString(OutputMode.Ansi)
                .Should()
                .Be($"one{Ansi.Color.Foreground.Red.EscapeSequence}two{Ansi.Color.Foreground.Default.EscapeSequence}three");
        }

        [Fact]
        public void Ansi_control_codes_can_be_included_in_interpolated_strings()
        {
            var writer = new StringWriter();

            var formatter = new TextSpanFormatter();

            var span = formatter.ParseToSpan($"{Ansi.Color.Foreground.LightGray}hello{Ansi.Text.AttributesOff}");

            writer.Write(span.ToString(OutputMode.Ansi));

            writer.ToString()
                  .Should()
                  .Be($"{Ansi.Color.Foreground.LightGray.EscapeSequence}hello{Ansi.Text.AttributesOff.EscapeSequence}");
        }
    }
}
