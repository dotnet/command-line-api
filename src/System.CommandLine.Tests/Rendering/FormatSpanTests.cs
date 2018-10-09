using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class FormatSpanTests
    {
        [Fact]
        public void ForegroundColorSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new ForegroundColorSpan("green");
            var two = new ForegroundColorSpan("green");

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void ForegroundColorSpans_with_the_same_name_are_equal()
        {
            var one = new ForegroundColorSpan("green");
            var two = new ForegroundColorSpan("green");

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
            var one = new ForegroundColorSpan("red");
            var two = new ForegroundColorSpan("green");

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void BackgroundColorSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new BackgroundColorSpan("green");
            var two = new BackgroundColorSpan("green");

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void BackgroundColorSpans_with_the_same_name_are_equal()
        {
            var one = new BackgroundColorSpan("green");
            var two = new BackgroundColorSpan("green");

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
            var one = new BackgroundColorSpan("red");
            var two = new BackgroundColorSpan("green");

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void A_ForegroundColorSpan_and_a_BackgroundColorSpan_having_the_same_name_are_not_equal()
        {
            var one = new ForegroundColorSpan("green");
            var two = new BackgroundColorSpan("green");

            one.Equals(two)
               .Should()
               .BeFalse();
        }

        [Fact]
        public void A_ForegroundColorSpan_and_a_BackgroundColorSpan_having_the_same_name_do_not_have_the_same_hash_code()
        {
            var one = new ForegroundColorSpan("green");
            var two = new BackgroundColorSpan("green");

            one.GetHashCode().Should().NotBe(two.GetHashCode());
        }

        [Fact]
        public void StyleSpans_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new StyleSpan("green");
            var two = new StyleSpan("green");

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void StyleSpans_with_the_same_name_are_equal()
        {
            var one = new StyleSpan("green");
            var two = new StyleSpan("green");

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
            var one = new StyleSpan("red");
            var two = new StyleSpan("green");

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
