// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class WrappingExtensionsTests
    {
        [Fact]
        public void SplitForWrapping_preserves_whitespace_after_words()
        {
            var input = "The quick brown\tfox\t jumps over    the lazy dog.";

            input.SplitForWrapping()
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
        public void SplitForWrapping_preserves_whitespace_at_the_end_of_the_string()
        {
            var input = "words and then space     ";

            input.SplitForWrapping()
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
        public void SplitForWrapping_preserves_whitespace_at_the_beginning_of_the_string()
        {
            var input = "    space and then words";

            input.SplitForWrapping()
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

        [Theory]
        [InlineData("\r\n")]
        [InlineData("\n")]
        public void SplitForWrapping_returns_newlines_as_distinct_elements(string newline)
        {
            var input = $"{newline}{newline}one two{newline}three{newline}";

            input.SplitForWrapping()
                 .Should()
                 .BeEquivalentTo(
                     new[] {
                         newline,
                         newline,
                         "one ",
                         "two",
                         newline,
                         "three",
                         newline,
                     },
                     options => options.WithStrictOrdering());
        }
    }
}
