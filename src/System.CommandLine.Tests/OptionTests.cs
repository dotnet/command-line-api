// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using FluentAssertions;
using System.Linq;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class OptionTests
    {
        [Fact]
        public void By_default_there_is_no_default_value()
        {
            Option<string> option = new("name");

            option.HasDefaultValue.Should().BeFalse();
        }

        [Fact]
        public void When_default_value_factory_is_set_then_HasDefaultValue_is_true()
        {
            Option<string[]> option = new("name");

            option.DefaultValueFactory = (_) => Array.Empty<string>();

            option.HasDefaultValue.Should().BeTrue();
        }

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
        public void Aliases_contains_prefixed_short_value()
        {
            var option = new Option<string>("--option", "-o");

            option.Aliases.Contains("-o").Should().BeTrue();
        }

        [Fact]
        public void Aliases_contains_prefixed_long_value()
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
            result.GetRequiredValue(optionA).Should().Be("value-for-a");
            result.GetRequiredValue<string>(optionA.Name).Should().Be("value-for-a");
            result.GetResult(optionB).Should().BeNull();
            result.Invoking(result => result.GetRequiredValue(optionB)).Should().Throw<InvalidOperationException>();
            result.Invoking(result => result.GetRequiredValue<string>(optionB.Name)).Should().Throw<InvalidOperationException>();
            result.GetValue(optionC).Should().Be("value-for-c");
            result.GetRequiredValue(optionC).Should().Be("value-for-c");
            result.GetRequiredValue<string>(optionC.Name).Should().Be("value-for-c");
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
                .GetResult(option)
                .GetValueOrDefault<int>()
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

            var result = new RootCommand { option }
                .Parse("")
                .GetResult(option);

            result
                .GetValueOrDefault<int>()
                .Should()
                .Be(123);

            result.GetRequiredValue(option)
                .Should()
                .Be(123);

            result.GetRequiredValue<int>(option.Name)
                .Should()
                .Be(123);
        }
        
        [Fact]
        public void Option_T_default_value_factory_can_be_set_after_instantiation()
        {
            var option = new Option<int>("-x");

            option.DefaultValueFactory = _ => 123;

            var parseResult = new RootCommand { option }.Parse("");

            parseResult
                .GetResult(option)
                .GetValueOrDefault<int>()
                .Should()
                .Be(123);
        }

        [Fact]
        public void When_there_is_no_default_value_then_GetRequiredValue_does_not_throw_for_bool()
        {
            var option = new Option<bool>("-x");

            var result = new RootCommand { option }.Parse("");

            using var _ = new AssertionScope();

            result.Invoking(r => r.GetRequiredValue(option)).Should().NotThrow();
            result.GetRequiredValue(option).Should().BeFalse();
            
            result.Invoking(r => r.GetRequiredValue<bool>("-x")).Should().NotThrow();
            result.GetRequiredValue<bool>("-x").Should().BeFalse();
            
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void GetRequiredValue_does_not_throw_when_help_is_requested_and_DefaultValueFactory_is_set()
        {
            var option = new Option<string>("-x")
            {
                DefaultValueFactory = _ => "default"
            };

            var result = new RootCommand { option }.Parse("-h");

            using var _ = new AssertionScope();

            result.Invoking(r => r.GetRequiredValue(option)).Should().NotThrow();
            result.GetRequiredValue(option).Should().Be("default");
            
            result.Invoking(r => r.GetRequiredValue<string>("-x")).Should().NotThrow();
            result.GetRequiredValue<string>("-x").Should().Be("default");
            
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_there_is_no_default_value_then_GetDefaultValue_does_not_throw_for_bool()
        {
            var option = new Option<bool>("-x");

            option.GetDefaultValue().Should().Be(false);
        }

        [Fact]
        public void When_there_is_a_default_value_then_GetRequiredValue_does_not_throw()
        {
            var option = new Option<string>("-x")
            {
                Required = true,
                DefaultValueFactory = _ => "default"
            };

            var result = new RootCommand { option }.Parse("");

            using var _ = new AssertionScope();

            result.Invoking(r => r.GetRequiredValue(option)).Should().NotThrow();
            result.Invoking(r => r.GetRequiredValue<string>("-x")).Should().NotThrow();
            result.GetRequiredValue(option).Should().Be("default");
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
            result.GetResult(option)
                .Should()
                .BeNull();
            result.GetValue(option)
                .Should()
                .BeNull();
        }
   
        [Fact]
        public void Option_of_boolean_defaults_to_false_when_not_specified()
        {
            var option = new Option<bool>("-x");

            var result = new RootCommand { option }.Parse("");

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

        [Fact]
        public void Option_result_provides_identifier_token_if_name_was_provided()
        {
            var option = new Option<int>("--name")
            {
                Aliases = { "-n" }
            };

            var result = new RootCommand { option }.Parse("--name 123");

            result.GetResult(option).IdentifierToken.Value.Should().Be("--name");
        }

        [Fact]
        public void Option_result_provides_identifier_token_if_alias_was_provided()
        {
            var option = new Option<int>("--name")
            {
                Aliases = { "-n" }
            };

            var result = new RootCommand { option }.Parse("-n 123");

            result.GetResult(option).IdentifierToken.Value.Should().Be("-n");
        }

        [Theory]
        [InlineData("--name 123", 1)]
        [InlineData("--name 123 --name 456", 2)]
        [InlineData("-n 123 --name 456", 2)]
        [InlineData("--name 123 -x different-option --name 456", 2)]
        public void Number_of_occurrences_of_identifier_token_is_exposed_by_option_result(string commandLine, int expectedCount)
        {
            var option = new Option<int>("--name")
            {
                Aliases = { "-n" }
            };

            var root = new RootCommand
            {
                option,
                new Option<string>("-x")
            };

            var optionResult = root.Parse(commandLine).GetResult(option);

            optionResult.IdentifierTokenCount.Should().Be(expectedCount);
        }

        [Fact] 
        public void Multiple_identifier_token_instances_without_argument_tokens_can_be_parsed()
        {
            var option = new Option<bool>("-v");

            var root = new RootCommand
            {
                option
            };

            var result = root.Parse("-v -v -v");

            using var _ = new AssertionScope();

            result.GetValue(option).Should().BeTrue();
            result.GetRequiredValue(option).Should().BeTrue();
            result.GetRequiredValue<bool>(option.Name).Should().BeTrue();
        }

        [Fact] 
        public void Multiple_bundled_identifier_token_instances_without_argument_tokens_can_be_parsed()
        {
            var option = new Option<bool>("-v");

            var root = new RootCommand
            {
                option
            };

            var result = root.Parse("-vvv");

            result.GetValue(option).Should().BeTrue();
        }

        [Theory] // https://github.com/dotnet/command-line-api/issues/669
        [InlineData("-vvv")]
        [InlineData("-v -v -v")]
        public void Custom_parser_can_be_used_to_implement_int_binding_based_on_token_count(string commandLine)
        {
            var option = new Option<int>("-v")
            {
                Arity = ArgumentArity.Zero,
                AllowMultipleArgumentsPerToken = true,
                CustomParser = argumentResult => ((OptionResult)argumentResult.Parent).IdentifierTokenCount,
            };

            var root = new RootCommand
            {
                option
            };

            var result = root.Parse(commandLine);

            result.GetValue(option).Should().Be(3);
        }
    }
}
