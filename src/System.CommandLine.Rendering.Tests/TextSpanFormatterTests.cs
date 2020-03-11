// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Rendering.Tests
{
    public class TextSpanFormatterTests
    {
        private readonly ITestOutputHelper output;

        public TextSpanFormatterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void A_simple_formattable_string_can_be_converted_to_a_ContentSpan()
        {
            var span = new TextSpanFormatter().ParseToSpan($"some text");

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
            var span = new TextSpanFormatter().ParseToSpan($"some {StyleSpan.BlinkOn()}blinking{StyleSpan.BlinkOff()} text");

            var containerSpan = span.Should()
                                    .BeOfType<ContainerSpan>()
                                    .Subject;

            containerSpan
                .Should()
                .BeEquivalentTo(
                    new ContainerSpan(
                        new ContentSpan("some "),
                        StyleSpan.BlinkOn(),
                        new ContentSpan("blinking"),
                        StyleSpan.BlinkOff(),
                        new ContentSpan(" text")
                    ),
                    options => options.WithStrictOrdering()
                                      .Excluding(s => s.Parent)
                                      .Excluding(s => s.Root)
                );
        }

        [Fact]
        public void Empty_strings_are_returned_as_empty_spans()
        {
            var formatter = new TextSpanFormatter();

            var span = formatter
                .ParseToSpan($"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default:a}");

            var containerSpan = span.Should()
                                    .BeOfType<ContainerSpan>()
                                    .Subject;

            containerSpan
                .Should()
                .BeEquivalentTo(
                    new ContainerSpan(
                        TextSpan.Empty(),
                        new ContentSpan("normal"),
                        TextSpan.Empty()
                    ),
                    options => options.WithStrictOrdering()
                                      .Excluding(s => s.Parent)
                                      .Excluding(s => s.Root)
                );
        }

        [Theory]
        [MemberData(nameof(FormattableStringsWithEscapes))]
        public void FormattableString_parsing_handles_escapes(
            FormattableString fs,
            int expectedCount)
        {
            var formatter = new TextSpanFormatter();

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
            var formatter = new TextSpanFormatter();

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

        [Fact]
        public void  When_formatting_null_values_then_empty_span_is_returned()
        {
            var formatter = new TextSpanFormatter();

            var span = formatter.Format(null);

            span.Should().Be(TextSpan.Empty());
        }

        public static IEnumerable<object[]> FormattableStringsWithFormatStrings()
        {
            var date = DateTime.Now;

            yield return Create($"{{ {date:dd/MM/yyyy}", 2);
            yield return Create($"some text and a {date:s}", 2);

            object[] Create(FormattableString fs, int expectedCount) =>
                new object[] {
                    fs,
                    expectedCount
                };
        }
    }
}
