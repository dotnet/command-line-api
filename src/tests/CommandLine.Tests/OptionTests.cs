// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
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
        public void Option_aliases_are_case_sensitive()
        {
            var option = new Option(new[] { "-o" }, "");

            option.HasAlias("O").Should().BeFalse();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_short_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "",
                Accept.NoArguments());

            option.HasAlias("-o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_unprefixed_short_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "",
                Accept.NoArguments());

            option.HasAlias("o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "",
                Accept.NoArguments());

            option.HasAlias("--option").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_unprefixed_long_value()
        {
            var option = new Option(
                new[] { "-o", "--option" }, "",
                Accept.NoArguments());

            option.HasAlias("option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new Option(
                new[] { "o" }, "",
                Accept.NoArguments());

            option.HasAlias("o").Should().BeTrue();
            option.HasAlias("-o").Should().BeTrue();
        }

        [Fact]
        public void An_option_must_have_at_least_one_alias()
        {
            Action create = () => new Option(Array.Empty<string>(), "", Accept.NoArguments());

            create.ShouldThrow<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An option must have at least one alias.");
        }

        [Fact]
        public void An_option_cannot_have_an_empty_alias()
        {
            Action create = () => new Option(new[] { "" }, "", Accept.NoArguments());

            create.ShouldThrow<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void An_option_cannot_have_an_alias_consisting_entirely_of_whitespace()
        {
            Action create = () => new Option(new[] { "  \t" }, "", Accept.NoArguments());

            create.ShouldThrow<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("An option alias cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void When_specifying_several_aliases_in_a_single_string_then_spaces_is_not_significant()
        {
            var option = Create.Option("-x | --exact", "");

            option.Parse("-x").HasOption("x").Should().BeTrue();
            option.Parse("--exact").HasOption("x").Should().BeTrue();
        }

        [Fact]
        public void Raw_aliases_are_exposed_by_an_option()
        {
            var option = Create.Option("-h|--help|/?", "");

            option.RawAliases
                  .Should()
                  .BeEquivalentTo("-h", "--help", "/?");
        }
    }
}