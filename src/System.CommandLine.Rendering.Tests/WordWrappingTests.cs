// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class WordWrappingTests
    {
        private readonly TestTerminal _terminal = new();

        [Theory]
        [MemberData(nameof(TestCases))]
        public void In_ansi_mode_word_wrap_wraps_correctly(RenderingTestCase @case)
        {
            new ConsoleRenderer(
                    _terminal,
                    OutputMode.Ansi)
                .RenderToRegion(
                    @case.InputSpan,
                    @case.Region);

            var output = _terminal.RenderOperations();

            output
                .Should()
                .BeEquivalentTo(
                    @case.ExpectedOutput,
                    options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData(0, 0, 5, 100)]
        [InlineData(0, 0, 10, 100)]
        [InlineData(0, 0, 10, 10)]
        [InlineData(0, 0, 20, 20)]
        [InlineData(0, 0, 500, 500)]
        public void Same_input_with_or_without_ansi_codes_produce_the_same_wrap(
            int left,
            int top,
            int width,
            int height)
        {
            var terminalWithoutAnsiCodes = new TestTerminal();
            var rendererWithoutAnsiCodes = new ConsoleRenderer(
                terminalWithoutAnsiCodes,
                OutputMode.NonAnsi);

            var terminalWithAnsiCodes = new TestTerminal();
            var rendererWithAnsiCodes = new ConsoleRenderer(
                terminalWithAnsiCodes,
                OutputMode.NonAnsi);

            FormattableString formattableString =
                $"Call me {StyleSpan.BoldOn()}{StyleSpan.UnderlinedOn()}Ishmael{StyleSpan.UnderlinedOff()}{StyleSpan.BoldOff()}. Some years ago -- never mind how long precisely -- having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world. It is a way I have of driving off the spleen and regulating the circulation. Whenever I find myself growing grim about the mouth; whenever it is a damp, drizzly November in my soul; whenever I find myself involuntarily pausing before coffin warehouses, and bringing up the rear of every funeral I meet; and especially whenever my hypos get such an upper hand of me, that it requires a strong moral principle to prevent me from deliberately stepping into the street, and {ForegroundColorSpan.Rgb(60, 0, 0)}methodically{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(90, 0, 0)}knocking{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(120, 0, 0)}people's{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(160, 0, 0)}hats{ForegroundColorSpan.Reset()} {ForegroundColorSpan.Rgb(220, 0, 0)}off{ForegroundColorSpan.Reset()} then, I account it high time to get to sea as soon as I can. This is my substitute for pistol and ball. With a philosophical flourish Cato throws himself upon his sword; I quietly take to the ship. There is nothing surprising in this. If they but knew it, almost all men in their degree, some time or other, cherish very nearly the same feelings towards the ocean with me.";

            var stringWithoutCodes = formattableString.ToString();

            var region = new Region(left, top, width, height);

            rendererWithAnsiCodes.RenderToRegion(
                formattableString,
                region);

            rendererWithoutAnsiCodes.RenderToRegion(
                stringWithoutCodes,
                region);

            var outputFromInputWithoutAnsiCodes = terminalWithoutAnsiCodes.Out.ToString();
            var outputFromInputWithAnsiCodes = terminalWithAnsiCodes.Out.ToString();

            outputFromInputWithAnsiCodes
                .Should()
                .Be(outputFromInputWithoutAnsiCodes);
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
                    Line(0, 0, "The"),
                    Line(0, 1, "qui"),
                    Line(0, 2, "bro"),
                    Line(0, 3, "fox"));

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    Line(12, 12, "The"),
                    Line(12, 13, "qui"),
                    Line(12, 14, "bro"),
                    Line(12, 15, "fox"));

                testCaseName = $"{nameof(ControlSpan)} at start of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{ForegroundColorSpan.Red()}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    Line(0, 0, $"{Ansi.Color.Foreground.Red.EscapeSequence}The"),
                    Line(0, 1, $"qui"),
                    Line(0, 2, $"bro"),
                    Line(0, 3, $"fox"));

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"{ForegroundColorSpan.Red()}The quick brown fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    Line(12, 12, $"{Ansi.Color.Foreground.Red.EscapeSequence}The"),
                    Line(12, 13, $"qui"),
                    Line(12, 14, $"bro"),
                    Line(12, 15, $"fox"));

                testCaseName = $"{nameof(ControlSpan)} at end of {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{ForegroundColorSpan.Reset()}",
                    inRegion: new Region(0, 0, 3, 4),
                    Line(0, 0, $"The"),
                    Line(0, 1, $"qui"),
                    Line(0, 2, $"bro"),
                    Line(0, 3, $"fox{Ansi.Color.Foreground.Default.EscapeSequence}"));

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick brown fox jumps over the lazy dog.{ForegroundColorSpan.Reset()}",
                    inRegion: new Region(12, 12, 3, 4),
                    Line(12, 12, $"The"),
                    Line(12, 13, $"qui"),
                    Line(12, 14, $"bro"),
                    Line(12, 15, $"fox{Ansi.Color.Foreground.Default.EscapeSequence}"));

                testCaseName = $"{nameof(ControlSpan)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset()} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, 3, 4),
                    Line(0, 0, $"The"),
                    Line(0, 1, $"qui{Ansi.Color.Foreground.Rgb(139, 69, 19).EscapeSequence}"),
                    Line(0, 2, $"bro{Ansi.Color.Foreground.Default.EscapeSequence}"),
                    Line(0, 3, $"fox"));

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset()} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, 3, 4),
                    Line(12, 12, $"The"),
                    Line(12, 13, $"qui{Ansi.Color.Foreground.Rgb(139, 69, 19).EscapeSequence}"),
                    Line(12, 14, $"bro{Ansi.Color.Foreground.Default.EscapeSequence}"),
                    Line(12, 15, $"fox"));

                testCaseName = $"{nameof(ControlSpan)}s around a word inside a {nameof(ContentSpan)}";

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset()} fox jumps over the lazy dog.",
                    inRegion: new Region(0, 0, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput:
                    Line(0, 0,
                         $"The quick {Ansi.Color.Foreground.Rgb(139, 69, 19).EscapeSequence}brown{Ansi.Color.Foreground.Default.EscapeSequence} fox jumps over the lazy dog."));

                yield return new RenderingTestCase(
                    name: testCaseName,
                    rendering: $"The quick {ForegroundColorSpan.Rgb(139, 69, 19)}brown{ForegroundColorSpan.Reset()} fox jumps over the lazy dog.",
                    inRegion: new Region(12, 12, "The quick brown fox jumps over the lazy dog.".Length, 1),
                    expectOutput:
                    Line(12, 12,
                         $"The quick {Ansi.Color.Foreground.Rgb(139, 69, 19).EscapeSequence}brown{Ansi.Color.Foreground.Default.EscapeSequence} fox jumps over the lazy dog."));
            }
        }

        private static TextRendered Line(int left, int top, string text) =>
            new(text, new Point(left, top));
    }
}
