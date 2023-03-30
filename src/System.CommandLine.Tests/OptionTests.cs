﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class OptionTests
    {
        [Fact]
        public void When_an_option_has_only_name_then_it_has_no_aliases()
        {
            var option = new CliOption<string>("myname");

            option.Name.Should().Be("myname");
            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void When_an_option_has_several_aliases_then_they_do_not_affect_its_name()
        {
            var option = new CliOption<string>(name: "m", aliases: new[] { "longer" });

            option.Name.Should().Be("m");
        }

        [Fact]
        public void Option_names_can_contain_prefix_characters()
        {
            var option = new CliOption<string>("--myname");

            option.Name.Should().Be("--myname");
        }

        [Fact]
        public void Aliases_is_aware_of_added_alias()
        {
            var option = new CliOption<string>("--original");

            option.Aliases.Add("--added");

            option.Aliases.Should().Contain("--added");
        }

        [Fact]
        public void RawAliases_is_aware_of_added_alias()
        {
            var option = new CliOption<string>("--original");

            option.Aliases.Add("--added");

            option.Aliases.Should().Contain("--added");
        }

        [Fact]
        public void A_prefixed_alias_can_be_added_to_an_option()
        {
            var option = new CliOption<string>("--apple");

            option.Aliases.Add("-a");

            option.Aliases.Contains("-a").Should().BeTrue();
        }

        [Fact]
        public void Option_aliases_are_case_sensitive()
        {
            var option = new CliOption<string>("name", "o");

            option.Aliases.Contains("O").Should().BeFalse();
        }

        [Fact]
        public void Aliases_accepts_prefixed_short_value()
        {
            var option = new CliOption<string>("--option", "-o");

            option.Aliases.Contains("-o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new CliOption<string>("-o", "--option");

            option.Aliases.Contains("--option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new CliOption<string>("o");

            option.Name.Should().Be("o");
            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void An_option_does_not_need_to_have_at_any_aliases()
        {
            var option = new CliOption<string>("justName");

            option.Aliases.Should().BeEmpty();
        }

        [Fact]
        public void An_option_cannot_have_an_empty_alias()
        {
            Action create = () => new CliOption<string>("name", "");

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
            Action create = () => new CliOption<string>("name", "  \t");

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
            var option = new CliOption<string>("--help", "-h", "/?");

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
            Action create = () => new CliOption<string>(name);

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
            var option = new CliOption<bool>("-x");

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
            var optionA = new CliOption<string>(prefix + "a");
            var optionB = new CliOption<string>(prefix + "b");
            var optionC = new CliOption<string>(prefix + "c");

            var rootCommand = new CliRootCommand
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
            var option = new CliOption<string>("--option", "-o")
            {
                Description = "desc"
            };

            option.Name.Should().Be("--option");
            option.Description.Should().Be("desc");
            option.Hidden.Should().BeFalse();
        }
        
        [Fact]
        public void Argument_retains_name_when_it_is_provided()
        {
            var option = new CliOption<string>("-alias")
            {
                HelpName = "arg"
            };

            option.HelpName.Should().Be("arg");
        }

        [Fact]
        public void Option_T_default_value_can_be_set_via_the_constructor()
        {
            var option = new CliOption<int>("-x")
            {
                DefaultValueFactory = _ => 123
            };

            new CliRootCommand { option }
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault<int>()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_can_be_set_after_instantiation()
        {
            var option = new CliOption<int>("-x")
            {
                DefaultValueFactory = (_) => 123
            };

            new CliRootCommand { option }
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault<int>()
                .Should()
                .Be(123);
        }
        
        [Fact]
        public void Option_T_default_value_factory_can_be_set_after_instantiation()
        {
            var option = new CliOption<int>("-x");

            option.DefaultValueFactory = (_) => 123;

            new CliRootCommand { option }
                .Parse("")
                .FindResultFor(option)
                .GetValueOrDefault<int>()
                .Should()
                .Be(123);
        }

        [Fact]
        public void Option_T_default_value_is_validated()
        {
            var option = new CliOption<int>("-x") { DefaultValueFactory = (_) => 123 };
            option.Validators.Add(symbol =>
                                    symbol.AddError(symbol.Tokens
                                                                .Select(t => t.Value)
                                                                .Where(v => v == "123")
                                                                .Select(_ => "ERR")
                                                                .First()));

            new CliRootCommand { option }
                .Parse("-x 123")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { "ERR" });
        }

        [Fact]
        public void Option_of_string_defaults_to_null_when_not_specified()
        {
            var option = new CliOption<string>("-x");

            var result = new CliRootCommand { option }.Parse("");
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
            var option = new CliOption<string>("-o", "-option");

            var parseResult = new CliRootCommand { option }.Parse(parseInput);

            parseResult.GetValue(option).Should().Be("value");
        }

        [Fact]
        public void Option_of_boolean_defaults_to_false_when_not_specified()
        {
            var option = new CliOption<bool>("-x");

            var result = new CliRootCommand { option }.Parse("");

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
            CliOption<ConsoleColor> option = new("--color");
            option.AcceptOnlyFromAmong(ConsoleColor.Red.ToString(), ConsoleColor.Green.ToString());

            var result = new CliRootCommand { option }.Parse("--color Fuschia");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { $"Argument 'Fuschia' not recognized. Must be one of:\n\t'Red'\n\t'Green'" });
        }

        [Fact]
        public void Every_option_can_provide_a_handler_and_it_takes_precedence_over_command_handler()
        {
            OptionAction optionAction = new();
            bool commandHandlerWasCalled = false;

            CliOption<bool> option = new("--test")
            {
                Action = optionAction,
            };
            CliCommand command = new CliCommand("cmd")
            {
                option
            };
            command.SetAction((_) =>
            {
                commandHandlerWasCalled = true;
            });

            ParseResult parseResult = command.Parse("cmd --test true");

            parseResult.Action.Should().NotBeNull();
            optionAction.WasCalled.Should().BeFalse();
            commandHandlerWasCalled.Should().BeFalse();

            parseResult.Invoke().Should().Be(0);
            optionAction.WasCalled.Should().BeTrue();
            commandHandlerWasCalled.Should().BeFalse();
        }

        internal sealed class OptionAction : CliAction
        {
            internal bool WasCalled = false;

            public override int Invoke(ParseResult context)
            {
                WasCalled = true;
                return 0;
            }

            public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken = default)
                => Task.FromResult(Invoke(context));
        }

        [Fact]
        public void When_multiple_options_with_handlers_are_parsed_only_the_last_one_is_effective()
        {
            OptionAction optionAction1 = new();
            OptionAction optionAction2 = new();
            OptionAction optionAction3 = new();
            
            CliCommand command = new CliCommand("cmd")
            {
                new CliOption<bool>("--1") { Action = optionAction1 },
                new CliOption<bool>("--2") { Action = optionAction2 },
                new CliOption<bool>("--3") { Action = optionAction3 }
            };

            ParseResult parseResult = command.Parse("cmd --1 true --3 false --2 true ");

            parseResult.Action.Should().Be(optionAction2);

            parseResult.Invoke().Should().Be(0);
            optionAction1.WasCalled.Should().BeFalse();
            optionAction2.WasCalled.Should().BeTrue();
            optionAction3.WasCalled.Should().BeFalse();
        }
    }
}
