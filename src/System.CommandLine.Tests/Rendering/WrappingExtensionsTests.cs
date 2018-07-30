using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class WrappingExtensionsTests
    {
        [Fact]
        public void SplitIntoWordsForWrapping_preserves_whitespace_after_words()
        {
            var input = "The quick brown\tfox\t jumps over    the lazy dog.";

            input.SplitIntoWordsForWrapping()
                 .Should()
                 .BeEquivalentTo(
                     new[] {
                         "The ",
                         "quick ",
                         "brown\t",
                         "fox\t ",
                         "jumps ",
                         "over    ",
                         "the ",
                         "lazy ",
                         "dog.",
                     },
                     options => options.WithStrictOrdering());
        }

        [Fact]
        public void SplitIntoWordsForWrapping_preserves_whitespace_at_the_end_of_the_string()
        {
            var input = "words and then space     ";

            input.SplitIntoWordsForWrapping()
                 .Should()
                 .BeEquivalentTo(
                     new[] {
                         "words ",
                         "and ",
                         "then ",
                         "space     ",
                     },
                     options => options.WithStrictOrdering());
        }

        [Fact]
        public void SplitIntoWordsForWrapping_preserves_whitespace_at_the_beginning_of_the_string()
        {
            var input = "    space and then words";

            input.SplitIntoWordsForWrapping()
                 .Should()
                 .BeEquivalentTo(
                     new[] {
                         "    ",
                         "space ",
                         "and ",
                         "then ",
                         "words",
                     },
                     options => options.WithStrictOrdering());
        }
    }
}
