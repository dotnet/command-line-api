// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class OptionTests
    {
        [Fact]
        public void When_an_option_has_only_one_alias_then_that_alias_is_its_name()
        {
            var option = new Option(new[] { "myname" }, "");

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void When_an_option_has_several_aliases_then_the_longest_alias_is_its_name()
        {
            var option = new Option(new[] { "myname", "m" }, "");

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void Option_names_do_not_contain_prefix_characters()
        {
            var option = new Option(new[] { "--myname", "m" }, "");

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void A_prefixed_alias_can_be_added_to_an_option()
        {
            var option = new Option("--apple");

            option.AddAlias("-a");

            option.HasAlias("a").Should().BeTrue();
            option.HasRawAlias("-a").Should().BeTrue();
        }

        [Fact]
        public void Option_aliases_are_case_sensitive()
        {
            var option = new Option(new[] { "-o" }, "");

            option.HasAlias("O").Should().BeFalse();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_short_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "");

            option.HasAlias("-o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_unprefixed_short_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "");

            option.HasAlias("o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "");

            option.HasAlias("--option").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_unprefixed_long_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "");

            option.HasAlias("option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new Option(
                new[] { "o" }, "");

            option.HasAlias("o").Should().BeTrue();
            option.HasAlias("-o").Should().BeTrue();
        }

        [Fact]
        public void An_option_must_have_at_least_one_alias()
        {
            Action create = () => new Option(Array.Empty<string>(), "");

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An option must have at least one alias.");
        }

        [Fact]
        public void An_option_cannot_have_an_empty_alias()
        {
            Action create = () => new Option(new[] { "" }, "");

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void An_option_cannot_have_an_alias_consisting_entirely_of_whitespace()
        {
            Action create = () => new Option(new[] { "  \t" }, "");

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void Raw_aliases_are_exposed_by_an_option()
        {
            var option = new Option(
                new[] { "-h", "--help", "/?" },
                "");

            option.RawAliases
                  .Should()
                  .BeEquivalentTo("-h", "--help", "/?");
        }

        [Theory]
        [InlineData(":", "-x{0}")]
        [InlineData("=", "-x{0}")]
        [InlineData(" ", "-x{0}")]
        [InlineData(":", "{0}-x")]
        [InlineData("=", "{0}-x")]
        [InlineData(" ", "{0}-x")]
        [InlineData(":", "--aa{0}aa")]
        [InlineData("=", "--aa{0}aa")]
        [InlineData(" ", "--aa{0}aa")]
        public void When_an_option_alias_contains_a_delimiter_then_an_informative_error_is_returned(
            string delimiter,
            string template)
        {
            Action create = () => new Parser(
                new Option(
                    string.Format(template, delimiter), ""));

            create.Should().Throw<ArgumentException>().Which.Message.Should()
                  .Be($"Symbol cannot contain delimiter: \"{delimiter}\"");
        }

        [Theory]
        [InlineData("-")]
        [InlineData("--")]
        [InlineData("/")]
        public void When_options_use_different_prefixes_they_still_work(string prefix)
        {
            var rootCommand = new RootCommand
                              {
                                  new Option(prefix + "a", "", new Argument<string>() { Name = "a" }),
                                  new Option(prefix + "b"),
                                  new Option(prefix + "c", "", new Argument<string>() { Name = "c"})
                              };
            var result = rootCommand.Parse(prefix + "c value-for-c " + prefix + "a value-for-a");

            result.ValueForOption(prefix + "a").Should().Be("value-for-a");
            result.ValueForOption(prefix + "c").Should().Be("value-for-c");
            result.HasOption(prefix + "b").Should().BeFalse();
        }

        [Fact]
        public void When_option_not_explicitly_provides_help_will_use_default_help()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "desc");

            option.Name.Should().Be("option");
            option.Description.Should().Be("desc");
            option.IsHidden.Should().BeFalse();
        }
    }
}
