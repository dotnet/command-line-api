using System.CommandLine.Rendering.Models;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class AnsiControlCodeTests
    {
        [Fact]
        public void Control_codes_with_equivalent_content_have_the_same_hash_code()
        {
            var one = new AnsiControlCode($"{Ansi.Esc}[s");
            var two = new AnsiControlCode($"{Ansi.Esc}[s");

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void Control_codes_with_equivalent_content_are_equal()
        {
            var one = new AnsiControlCode($"{Ansi.Esc}[s");
            var two = new AnsiControlCode($"{Ansi.Esc}[s");

            one.Equals(two)
               .Should()
               .BeTrue();

            one.Invoking(code => code.Equals(null))
               .Should()
               .NotThrow<NullReferenceException>();
        }

        [Fact]
        public void Control_codes_with_nonequivalent_content_are_not_equal()
        {
            var one = new AnsiControlCode($"{Ansi.Esc}[s");
            var two = new AnsiControlCode($"{Ansi.Esc}[u");

            one.Equals(two)
               .Should()
               .BeFalse();
        }
    }
}
