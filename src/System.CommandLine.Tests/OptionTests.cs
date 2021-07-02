﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class OptionTests : SymbolTests
    {
        [Fact]
        public void When_an_option_has_only_one_alias_then_that_alias_is_its_name()
        {
            var option = new Option(new[] { "myname" });

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void When_an_option_has_several_aliases_then_the_longest_alias_is_its_name()
        {
            var option = new Option(new[] { "myname", "m" });

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void Option_names_do_not_contain_prefix_characters()
        {
            var option = new Option(new[] { "--myname", "m" });

            option.Name.Should().Be("myname");
        }

        [Fact]
        public void Aliases_is_aware_of_added_alias()
        {
            var option = new Option("--original");

            option.AddAlias("--added");

            option.Aliases.Should().Contain("--added");
            option.HasAlias("--added").Should().BeTrue();
        }

        [Fact]
        public void RawAliases_is_aware_of_added_alias()
        {
            var option = new Option("--original");

            option.AddAlias("--added");

            option.Aliases.Should().Contain("--added");
            option.HasAlias("--added").Should().BeTrue();
        }

        [Fact]
        public void A_prefixed_alias_can_be_added_to_an_option()
        {
            var option = new Option("--apple");

            option.AddAlias("-a");

            option.HasAliasIgnorePrefix("a").Should().BeTrue();
            option.HasAlias("-a").Should().BeTrue();
        }

        [Fact]
        public void Option_aliases_are_case_sensitive()
        {
            var option = new Option(new[] { "-o" });

            option.HasAlias("O").Should().BeFalse();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_short_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAlias("-o").Should().BeTrue();
        }

        [Fact]
        public void HasAliasIgnorePrefix_accepts_unprefixed_short_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAliasIgnorePrefix("o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAlias("--option").Should().BeTrue();
        }

        [Fact]
        public void HasAliasIgnorePrefix_accepts_unprefixed_long_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAliasIgnorePrefix("option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new Option(new[] { "o" });

            option.HasAlias("o").Should().BeTrue();
        }

        [Fact]
        public void An_option_must_have_at_least_one_alias()
        {
            Action create = () => new Option(Array.Empty<string>());

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain("An option must have at least one alias");
        }

        [Fact]
        public void An_option_cannot_have_an_empty_alias()
        {
            Action create = () => new Option(new[] { "" });

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
            Action create = () => new Option(new[] { "  \t" });

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
            var option = new Option(new[] { "-h", "--help", "/?" });

            option.Aliases
                  .Should()
                  .BeEquivalentTo("-h", "--help", "/?");
        }

        [Theory]
        [InlineData("-x ")]
        [InlineData(" -x")]
        [InlineData("--aa aa")]
        public void When_an_option_is_created_with_an_alias_that_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            Action create = () => new Option(alias);

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain($"Option alias cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("-x ")]
        [InlineData(" -x")]
        [InlineData("--aa aa")]
        public void When_an_option_alias_is_added_and_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            var option = new Option("-x");

            Action addAlias = () => option.AddAlias(alias);

            addAlias.Should()
                    .Throw<ArgumentException>()
                    .Which
                    .Message
                    .Should()
                    .Contain($"Option alias cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("-")]
        [InlineData("--")]
        [InlineData("/")]
        public void When_options_use_different_prefixes_they_still_work(string prefix)
        {
            var optionA = new Option<string>(prefix + "a");
            var optionB = new Option(prefix + "b");
            var optionC = new Option<string>(prefix + "c");

            var rootCommand = new RootCommand
                              {
                                  optionA,
                                  optionB,
                                  optionC
                              };

            var result = rootCommand.Parse(prefix + "c value-for-c " + prefix + "a value-for-a");

            result.ValueForOption(optionA).Should().Be("value-for-a");
            result.HasOption(optionB).Should().BeFalse();
            result.ValueForOption(optionC).Should().Be("value-for-c");
        }

        [Fact]
        public void When_option_not_explicitly_provides_help_will_use_default_help()
        {
            var option = new Option(new[] { "-o", "--option" }, "desc");

            option.Name.Should().Be("option");
            option.Description.Should().Be("desc");
            option.IsHidden.Should().BeFalse();
        }
        
        [Fact]
        public void Argument_takes_option_alias_as_its_name_when_it_is_not_provided()
        {
            var command = new Option("--alias", arity: ArgumentArity.ZeroOrOne);

            command.ArgumentHelpName.Should().Be("alias");
        }

        [Fact]
        public void Argument_retains_name_when_it_is_provided()
        {
            var option = new Option("-alias", arity: ArgumentArity.ZeroOrOne)
            {
                ArgumentHelpName = "arg"
            };

            option.ArgumentHelpName.Should().Be("arg");
        }

        [Fact]
        public void Option_T_default_value_can_be_set_via_the_constructor()
        {
            var option = new Option<int>(
                "-x",
                parseArgument: parsed => 123,
                isDefault: true);

            option
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_can_be_set_after_instantiation()
        {
            var option = new Option<int>("-x");

            option.SetDefaultValue(123);

            option
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault()
                .Should()
                .Be(123);
        }
        
        [Fact]
        public void Option_T_default_value_factory_can_be_set_after_instantiation()
        {
            var option = new Option<int>("-x");

            option.SetDefaultValueFactory(() => 123);

            option
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_is_validated()
        {
            var option = new Option<int>("-x", () => 123);
            option.AddValidator( symbol =>
                    symbol.Tokens
                    .Select(t => t.Value)
                    .Where(v => v == "123")
                    .Select(x => "ERR")
                    .FirstOrDefault());


            option
                .Parse("-x 123")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { "ERR" });
        }

        [Fact]
        public void Option_of_string_defaults_to_empty_when_not_specified()
        {
            var option = new Option<string>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_IEnumerable_of_T_defaults_to_empty_when_not_specified()
        {
            var option = new Option<IEnumerable<string>>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse(); 
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_Array_defaults_to_empty_when_not_specified()
        {
            var option = new Option<string[]>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_List_defaults_to_empty_when_not_specified()
        {
            var option = new Option<List<string>>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_IList_of_T_defaults_to_empty_when_not_specified()
        {
            var option = new Option<IList<string>>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_IList_defaults_to_empty_when_not_specified()
        {
            var option = new Option<System.Collections.IList>("-x");
            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_ICollection_defaults_to_empty_when_not_specified()
        {
            var option = new Option<System.Collections.ICollection>("-x");
            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_IEnumerable_defaults_to_empty_when_not_specified()
        {
            var option = new Option<System.Collections.IEnumerable>("-x");
            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Option_of_ICollection_of_T_defaults_to_empty_when_not_specified()
        {
            var option = new Option<ICollection<string>>("-x");

            var result = option.Parse("");
            result.HasOption(option)
                .Should()
                .BeFalse();
            result.ValueForOption(option)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Single_option_arg_is_matched_when_disallowing_multiple_args_per_option_token()
        {
            var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
            var command = new Command("the-command") { option };

            var result = command.Parse("--option 1 2");

            var optionResult = result.ValueForOption(option);

            optionResult.Should().BeEquivalentTo(new[] { "1" });
            result.UnmatchedTokens.Should().BeEquivalentTo(new[] { "2" });
        }

        [Fact]
        public void Multiple_option_args_are_matched_with_multiple_option_tokens_when_disallowing_multiple_args_per_option_token()
        {
            var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
            var command = new Command("the-command") { option };

            var result = command.Parse("--option 1 --option 2");

            var optionResult = result.ValueForOption(option);

            optionResult.Should().BeEquivalentTo(new[] { "1", "2" });
        }

        [Fact]
        public void When_Name_is_set_to_its_current_value_then_it_is_not_removed_from_aliases()
        {
            var option = new Option("--name");

            option.Name = "name";

            option.HasAlias("name").Should().BeTrue();
            option.HasAlias("--name").Should().BeTrue();
            option.Aliases.Should().Contain("--name");
            option.Aliases.Should().Contain("name");
        }

        [Theory]
        [InlineData("-option value")]
        [InlineData("-option:value")]
        public void When_aliases_overlap_the_longer_alias_is_chosen(string parseInput)
        {
            var option = new Option<string>(new[] { "-o", "-option" });

            var parseResult = option.Parse(parseInput);

            parseResult.ValueForOption(option).Should().Be("value");
        }

        [Fact]
        public void Option_of_boolean_defaults_to_false_when_not_specified()
        {
            var option = new Option<bool>("-x");

            var result = option.Parse("");

            result.HasOption(option)
                  .Should()
                  .BeFalse();
            result.ValueForOption(option)
                  .Should()
                  .BeFalse();
        }

        [Fact]
        public void Arity_of_non_generic_option_defaults_to_zero()
        {
            var option = new Option("-x");

            option.Arity.Should().BeEquivalentTo(ArgumentArity.Zero);
        }

        protected override Symbol CreateSymbol(string name) => new Option(name);
    }
}
