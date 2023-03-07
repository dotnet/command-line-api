// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class OptionTests
    {
        [Fact]
        public void When_an_option_has_only_name_then_it_has_no_aliases()
        {
            var option = new Option<string>("myname");

            option.Name.Should().Be("myname");
            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void When_an_option_has_several_aliases_then_they_do_not_affect_its_name()
        {
            var option = new Option<string>(name: "m", aliases: new[] { "longer" });

            option.Name.Should().Be("m");
        }

        [Fact]
        public void Option_names_can_contain_prefix_characters()
        {
            var option = new Option<string>("--myname");

            option.Name.Should().Be("--myname");
        }

        [Fact]
        public void Aliases_is_aware_of_added_alias()
        {
            var option = new Option<string>("--original");

            option.Aliases.Add("--added");

            option.Aliases.Should().Contain("--added");
        }

        [Fact]
        public void RawAliases_is_aware_of_added_alias()
        {
            var option = new Option<string>("--original");

            option.Aliases.Add("--added");

            option.Aliases.Should().Contain("--added");
        }

        [Fact]
        public void A_prefixed_alias_can_be_added_to_an_option()
        {
            var option = new Option<string>("--apple");

            option.Aliases.Add("-a");

            option.Aliases.Contains("-a").Should().BeTrue();
        }

        [Fact]
        public void Option_aliases_are_case_sensitive()
        {
            var option = new Option<string>("name", "o");

            option.Aliases.Contains("O").Should().BeFalse();
        }

        [Fact]
        public void Aliases_accepts_prefixed_short_value()
        {
            var option = new Option<string>("--option", "-o");

            option.Aliases.Contains("-o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new Option<string>("-o", "--option");

            option.Aliases.Contains("--option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new Option<string>("o");

            option.Name.Should().Be("o");
            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void An_option_does_not_need_to_have_at_any_aliases()
        {
            var option = new Option<string>("justName");

            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void An_option_cannot_have_an_empty_alias()
        {
            Action create = () => new Option<string>("name", "");

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("Names and aliases cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void An_option_cannot_have_an_alias_consisting_entirely_of_whitespace()
        {
            Action create = () => new Option<string>("name", "  \t");

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("Names and aliases cannot be null, empty, or consist entirely of whitespace.");
        }

        [Fact]
        public void Raw_aliases_are_exposed_by_an_option()
        {
            var option = new Option<string>("--help", "-h", "/?");

            option.Aliases
                  .Should()
                  .BeEquivalentTo("-h", "/?");
        }

        [Theory]
        [InlineData("-x ")]
        [InlineData(" -x")]
        [InlineData("--aa aa")]
        public void When_an_option_is_created_with_a_name_that_contains_whitespace_then_an_informative_error_is_returned(
            string name)
        {
            Action create = () => new Option<string>(name);

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain($"Names and aliases cannot contain whitespace: \"{name}\"");
        }

        [Theory]
        [InlineData("-x ")]
        [InlineData(" -x")]
        [InlineData("--aa aa")]
        public void When_an_option_alias_is_added_and_contains_whitespace_then_an_informative_error_is_returned(string alias)
        {
            var option = new Option<bool>("-x");

            Action addAlias = () => option.Aliases.Add(alias);

            addAlias.Should()
                    .Throw<ArgumentException>()
                    .Which
                    .Message
                    .Should()
                    .Contain($"Names and aliases cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("-")]
        [InlineData("--")]
        [InlineData("/")]
        public void When_options_use_different_prefixes_they_still_work(string prefix)
        {
            var optionA = new Option<string>(prefix + "a");
            var optionB = new Option<string>(prefix + "b");
            var optionC = new Option<string>(prefix + "c");

            var rootCommand = new RootCommand
                              {
                                  optionA,
                                  optionB,
                                  optionC
                              };

            var result = rootCommand.Parse(prefix + "c value-for-c " + prefix + "a value-for-a");

            result.GetValue(optionA).Should().Be("value-for-a");
            result.FindResultFor(optionB).Should().BeNull();
            result.GetValue(optionC).Should().Be("value-for-c");
        }

        [Fact]
        public void When_option_not_explicitly_provides_help_will_use_default_help()
        {
            var option = new Option<string>("--option", "-o")
            {
                Description = "desc"
            };

            option.Name.Should().Be("--option");
            option.Description.Should().Be("desc");
            option.IsHidden.Should().BeFalse();
        }
        
        [Fact]
        public void Argument_retains_name_when_it_is_provided()
        {
            var option = new Option<string>("-alias")
            {
                HelpName = "arg"
            };

            option.HelpName.Should().Be("arg");
        }

        [Fact]
        public void Option_T_default_value_can_be_set_via_the_constructor()
        {
            var option = new Option<int>("-x")
            {
                DefaultValueFactory = _ => 123
            };

            new RootCommand { option }
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_can_be_set_after_instantiation()
        {
            var option = new Option<int>("-x")
            {
                DefaultValueFactory = (_) => 123
            };

            new RootCommand { option }
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

            option.DefaultValueFactory = (_) => 123;

            new RootCommand { option }
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_is_validated()
        {
            var option = new Option<int>("-x") { DefaultValueFactory = (_) => 123 };
            option.Validators.Add(symbol =>
                                    symbol.AddError(symbol.Tokens
                                                                .Select(t => t.Value)
                                                                .Where(v => v == "123")
                                                                .Select(_ => "ERR")
                                                                .First()));

            new RootCommand { option }
                .Parse("-x 123")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { "ERR" });
        }

        [Fact]
        public void Option_of_string_defaults_to_null_when_not_specified()
        {
            var option = new Option<string>("-x");

            var result = new RootCommand { option }.Parse("");
            result.FindResultFor(option)
                .Should()
                .BeNull();
            result.GetValue(option)
                .Should()
                .BeNull();
        }
        
        [Theory]
        [InlineData("-option value")]
        [InlineData("-option:value")]
        public void When_aliases_overlap_the_longer_alias_is_chosen(string parseInput)
        {
            var option = new Option<string>("-o", "-option");

            var parseResult = new RootCommand { option }.Parse(parseInput);

            parseResult.GetValue(option).Should().Be("value");
        }

        [Fact]
        public void Option_of_boolean_defaults_to_false_when_not_specified()
        {
            var option = new Option<bool>("-x");

            var result = new RootCommand { option }.Parse("");

            result.FindResultFor(option)
                .Should()
                .BeNull();
            result.GetValue(option)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Option_of_enum_can_limit_enum_members_as_valid_values()
        {
            Option<ConsoleColor> option = new("--color");
            option.AcceptOnlyFromAmong(ConsoleColor.Red.ToString(), ConsoleColor.Green.ToString());

            var result = new RootCommand { option }.Parse("--color Fuschia");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { $"Argument 'Fuschia' not recognized. Must be one of:\n\t'Red'\n\t'Green'" });
        }
    }
}
