// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class ControlSpanTests
    {
        [Fact]
        public void ForegroundColorSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void ForegroundColorSpans_with_the_same_name_are_equal()
        {
            var one = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeTrue();

            one.Invoking(code => code.Equals(null))
               .Should()
               .NotThrow<NullReferenceException>();
        }

        [Fact]
        public void ForegroundColorSpans_with_different_names_are_not_equal()
        {
            var one = new ForegroundColorSpan("red", Ansi.Color.Foreground.Green);
            var two = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void BackgroundColorSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void BackgroundColorSpans_with_the_same_name_are_equal()
        {
            var one = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeTrue();

            one.Invoking(code => code.Equals(null))
               .Should()
               .NotThrow<NullReferenceException>();
        }

        [Fact]
        public void BackgroundColorSpans_with_different_names_are_not_equal()
        {
            var one = new BackgroundColorSpan("red", Ansi.Color.Foreground.Red);
            var two = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void A_ForegroundColorSpan_and_a_BackgroundColorSpan_having_the_same_name_are_not_equal()
        {
            var one = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void A_ForegroundColorSpan_and_a_BackgroundColorSpan_having_the_same_name_do_not_have_the_same_hash_code()
        {
            var one = new ForegroundColorSpan("green", Ansi.Color.Foreground.Green);
            var two = new BackgroundColorSpan("green", Ansi.Color.Foreground.Green);

            one.GetHashCode().Should().NotBe(two.GetHashCode());
        }

        [Fact]
        public void StyleSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new StyleSpan("green", Ansi.Color.Foreground.Green);
            var two = new StyleSpan("green", Ansi.Color.Foreground.Green);

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void StyleSpans_with_the_same_name_are_equal()
        {
            var one = new StyleSpan("green", Ansi.Color.Foreground.Green);
            var two = new StyleSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeTrue();

            one.Invoking(code => code.Equals(null))
               .Should()
               .NotThrow<NullReferenceException>();
        }

        [Fact]
        public void StyleSpans_with_different_names_are_not_equal()
        {
            var one = new StyleSpan("red", Ansi.Color.Foreground.Green);
            var two = new StyleSpan("green", Ansi.Color.Foreground.Green);

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void FormatSpans_do_not_have_default_string_representations()
        {
            $"{ForegroundColorSpan.DarkGray()}The {BackgroundColorSpan.Cyan()}quick{StyleSpan.BlinkOn()} brown fox jumped over the lazy dog.{StyleSpan.BoldOff()}{ForegroundColorSpan.Reset()}{BackgroundColorSpan.Reset()}"
                .Should()
                .Be("The quick brown fox jumped over the lazy dog.");
        }
    }
}
