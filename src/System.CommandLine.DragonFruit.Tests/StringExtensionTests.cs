// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("Option123", "option123")]
        [InlineData("dWORD", "d-word")]
        [InlineData("MSBuild", "msbuild")]
        [InlineData("NoEdit", "no-edit")]
        [InlineData("SetUpstreamBranch", "set-upstream-branch")]
        [InlineData("lowerCaseFirst", "lower-case-first")]
        [InlineData("_field", "field")]
        [InlineData("__field", "field")]
        [InlineData("___field", "field")]
        [InlineData("m_field", "m-field")]
        [InlineData("m_Field", "m-field")]
        public void ToKebabCase(string input, string expected) => input.ToKebabCase().Should().Be(expected);

        [Fact]
        public void Wrap_chops_long_words()
        {
            var longWord = "looooooooooong";
            var input = $"{longWord}";

            var lines = input.Wrap(4);

            lines.First()
                 .Should()
                 .Be("looo");
        }

        [Fact]
        public void Wrap_pads_the_available_space_to_the_specified_width()
        {
            var longWord = "looooooooooong";
            var shortWord = "short";
            var input = $"{shortWord} {longWord}";

            var lines = input.Wrap(longWord.Length, 2);

            lines.First()
                 .Should()
                 .Be("short         ");
        }

        [Fact]
        public void Wrap_does_not_wrap_to_more_lines_than_specified()
        {
            var input = "The quick brown fox jumps over the lazy dog.";

            var lines = input.Wrap(6, 2);

            lines.Should()
                 .BeEquivalentTo(
                     new[] {
                         "The   ",
                         "quick "
                     },
                     options => options.WithStrictOrdering());
        }
    }
}
