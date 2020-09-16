// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            option.Aliases.Should().Contain("added");
            option.HasAlias("added").Should().BeTrue();
        }

        [Fact]
        public void RawAliases_is_aware_of_added_alias()
        {
            var option = new Option("--original");

            option.AddAlias("--added");

            option.RawAliases.Should().Contain("--added");
            option.HasRawAlias("--added").Should().BeTrue();
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
        public void HasAlias_accepts_unprefixed_short_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAlias("o").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_prefixed_long_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAlias("--option").Should().BeTrue();
        }

        [Fact]
        public void HasAlias_accepts_unprefixed_long_value()
        {
            var option = new Option(new[] { "-o", "--option" });

            option.HasAlias("option").Should().BeTrue();
        }

        [Fact]
        public void It_is_not_necessary_to_specify_a_prefix_when_adding_an_option()
        {
            var option = new Option(new[] { "o" });

            option.HasAlias("o").Should().BeTrue();
            option.HasAlias("-o").Should().BeTrue();
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

            option.RawAliases
                  .Should()
                  .BeEquivalentTo("-h", "--help", "/?");
        }

        [Theory]
        [InlineData(":", "-x{0}")]
        [InlineData("=", "-x{0}")]
        [InlineData(":", "{0}-x")]
        [InlineData("=", "{0}-x")]
        [InlineData(":", "--aa{0}aa")]
        [InlineData("=", "--aa{0}aa")]
        public void When_an_option_alias_contains_a_delimiter_then_an_informative_error_is_returned(
            string delimiter,
            string template)
        {
            var alias = string.Format(template, delimiter);

            Action create = () =>
            {
                new Parser(new Option(alias));
            };

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be($"Option \"{alias}\" is not allowed to contain a delimiter but it contains \"{delimiter}\"");
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
            var rootCommand = new RootCommand
                              {
                                  new Option(prefix + "a")
                                  {
                                      Argument = new Argument<string>()
                                  },
                                  new Option(prefix + "b"),
                                  new Option(prefix + "c")
                                  {
                                      Argument = new Argument<string>()
                                  }
                              };
            var result = rootCommand.Parse(prefix + "c value-for-c " + prefix + "a value-for-a");

            result.ValueForOption(prefix + "a").Should().Be("value-for-a");
            result.ValueForOption(prefix + "c").Should().Be("value-for-c");
            result.HasOption(prefix + "b").Should().BeFalse();
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
            var command = new Option("--alias")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            command.Argument.Name.Should().Be("alias");
        }

        [Fact]
        public void Argument_retains_name_when_it_is_provided()
        {
            var option = new Option("-alias")
            {
                Argument = new Argument
                {
                    Name = "arg",
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            option.Argument.Name.Should().Be("arg");
        }

        [Fact]
        public void Option_T_Argument_returns_an_Argument_T_when_not_explicitly_initialized()
        {
            var option = new Option<int>("-i");

            option.Argument.Should().BeOfType<Argument<int>>();
        }

        [Theory]
        [InlineData(typeof(Argument))]
        [InlineData(typeof(Argument<string>))]
        public void Option_T_Argument_cannot_be_set_to_Argument_of_incorrect_type(Type argumentType)
        {
            var option = new Option<int>("i");

            var argument = Activator.CreateInstance(argumentType);

            option.Invoking(o => o.Argument = (Argument) argument)
                  .Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be($"Argument must be of type {typeof(Argument<int>)} but was {argument.GetType()}");
        }

        [Fact]
        public void Option_T_default_value_can_be_set()
        {
            var option = new Option<int>(
                "-x",
                parseArgument: parsed => 123,
                isDefault: true);

            var result = option
                         .Parse("")
                         .FindResultFor(option)
                         .GetValueOrDefault()
                         .Should()
                         .Be(123);
        }

        [Fact]
        public void Option_T_default_value_is_validated()
        {
            var arg = new Argument<int>(() => 123);
            arg.AddValidator( symbol =>
                    symbol.Tokens
                    .Select(t => t.Value)
                    .Where(v => v == "123")
                    .Select(x => "ERR")
                    .FirstOrDefault());

            var option = new Option("-x")
            { 
                Argument = arg
            };

            option
                .Parse("-x 123")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(new[] { "ERR" });
        }

        [Fact]
        public void When_Name_is_set_to_its_current_value_then_it_is_not_removed_from_aliases()
        {
            var option = new Option("--name");

            option.Name = "name";

            option.HasAlias("name").Should().BeTrue();
            option.HasAlias("--name").Should().BeTrue();
            option.RawAliases.Should().Contain("--name");
            option.Aliases.Should().Contain("name");
        }

        protected override Symbol CreateSymbol(string name) => new Option(name);
    }
}
