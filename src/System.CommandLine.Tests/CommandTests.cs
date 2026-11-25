// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly Command _outerCommand, _lazyOuterCommand;

        public CommandTests()
        {
            _outerCommand = new Command("outer")
            {
                new Command("inner")
                {
                    new Option<string>("--option")
                }
            };
            _lazyOuterCommand = new Command("outer");
            _lazyOuterCommand.Add("inner", static () => new Command("inner")
            {
                new Option<string>("--option")
            });
        }

        // Returns the command created by the factory, either directly or by registering it lazily in a parent command and materializing it.
        private static Command CreateCommand(bool useLazy, string name, Func<Command> factory)
        {
            if (!useLazy)
            {
                return factory();
            }

            Command parent = new("parent");
            parent.Add(name, factory);
            return parent.Subcommands.Single();
        }

        private static void AddSubcommand(Command parent, bool useLazy, string name, Func<Command> factory)
        {
            if (useLazy)
            {
                parent.Add(name, factory);
            }
            else
            {
                parent.Add(factory());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Outer_command_is_identified_correctly_by_RootCommand(bool useLazy)
        {
            Command command = useLazy ? _lazyOuterCommand : _outerCommand;
            ParseResult result = command.Parse("outer inner --option argument1");

            result
                .RootCommandResult
                .Command
                .Name
                .Should()
                .Be("outer");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Outer_command_is_identified_correctly_by_Parent_property(bool useLazy)
        {
            Command command = useLazy ? _lazyOuterCommand : _outerCommand;
            ParseResult result = command.Parse("outer inner --option argument1");

            result
                .CommandResult
                .Parent
                .Should()
                .BeOfType<CommandResult>()
                .Which
                .Command
                .Name
                .Should()
                .Be("outer");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Inner_command_is_identified_correctly(bool useLazy)
        {
            Command command = useLazy ? _lazyOuterCommand : _outerCommand;
            ParseResult result = command.Parse("outer inner --option argument1");

            result.CommandResult
                  .Should()
                  .BeOfType<CommandResult>()
                  .Which
                  .Command
                  .Name
                  .Should()
                  .Be("inner");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Inner_command_option_is_identified_correctly(bool useLazy)
        {
            Command command = useLazy ? _lazyOuterCommand : _outerCommand;
            ParseResult result = command.Parse("outer inner --option argument1");

            result.CommandResult
                  .Children
                  .ElementAt(0)
                  .Should()
                  .BeOfType<OptionResult>()
                  .Which
                  .Option
                  .Name
                  .Should()
                  .Be("--option");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Inner_command_option_argument_is_identified_correctly(bool useLazy)
        {
            Command command = useLazy ? _lazyOuterCommand : _outerCommand;
            ParseResult result = command.Parse("outer inner --option argument1");

            result.CommandResult
                  .Children
                  .ElementAt(0)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("argument1");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Commands_at_multiple_levels_can_have_their_own_arguments(bool useLazy)
        {
            Command outer = new("outer")
            {
                new Argument<string>("outer_arg")
            };
            AddSubcommand(outer, useLazy, "inner", static () =>
                new Command("inner")
                {
                    new Argument<string[]>("inner_arg")
                });

            ParseResult result = outer.Parse("outer arg1 inner arg2 arg3");

            result.CommandResult
                  .Parent
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg1");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Aliases_is_aware_of_added_alias(bool useLazy)
        {
            Command command = CreateCommand(useLazy, "original", static () => new Command("original"));

            command.Aliases.Add("added");

            command.Aliases.Should().Contain("added");
        }


        [Theory]
        [InlineData("aa ", true)]
        [InlineData("aa ", false)]
        [InlineData(" aa", true)]
        [InlineData(" aa", false)]
        [InlineData("aa aa", true)]
        [InlineData("aa aa", false)]
        public void When_a_command_is_created_with_an_alias_that_contains_whitespace_then_an_informative_error_is_returned(
            string alias, bool useLazy)
        {
            Action create = () => CreateCommand(useLazy, alias, () => new Command(alias));

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain($"Names and aliases cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("aa ", true)]
        [InlineData("aa ", false)]
        [InlineData(" aa", true)]
        [InlineData(" aa", false)]
        [InlineData("aa aa", true)]
        [InlineData("aa aa", false)]
        public void When_a_command_alias_is_added_and_contains_whitespace_then_an_informative_error_is_returned(
            string alias, bool useLazy)
        {
            Command parent = new("parent");

            Action addAlias = useLazy
                ? () => parent.Add("-x", [alias], static () => new Command("-x"))
                : () => new Command("-x").Aliases.Add(alias);

            addAlias
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Contain($"Names and aliases cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("outer", "outer", true)]
        [InlineData("outer", "outer", false)]
        [InlineData("outer arg", "outer", true)]
        [InlineData("outer arg", "outer", false)]
        [InlineData("outer inner", "inner", true)]
        [InlineData("outer inner", "inner", false)]
        [InlineData("outer arg inner", "inner", true)]
        [InlineData("outer arg inner", "inner", false)]
        [InlineData("outer arg inner arg", "inner", true)]
        [InlineData("outer arg inner arg", "inner", false)]
        [InlineData("outer sibling", "sibling", true)]
        [InlineData("outer sibling", "sibling", false)]
        [InlineData("outer inner inner-er", "inner-er", true)]
        [InlineData("outer inner inner-er", "inner-er", false)]
        [InlineData("outer inner arg inner-er", "inner-er", true)]
        [InlineData("outer inner arg inner-er", "inner-er", false)]
        [InlineData("outer inner arg inner-er arg", "inner-er", true)]
        [InlineData("outer inner arg inner-er arg", "inner-er", false)]
        [InlineData("outer arg inner arg inner-er arg", "inner-er", true)]
        [InlineData("outer arg inner arg inner-er arg", "inner-er", false)]
        public void ParseResult_Command_identifies_innermost_command(string input, string expectedCommand, bool useLazy)
        {
            Command outer = new("outer");
            AddSubcommand(outer, useLazy, "inner", () =>
            {
                Command inner = new("inner");
                AddSubcommand(inner, useLazy, "inner-er", static () => new Command("inner-er"));
                return inner;
            });
            AddSubcommand(outer, useLazy, "sibling", static () => new Command("sibling"));

            ParseResult result = outer.Parse(input);

            result.CommandResult.Command.Name.Should().Be(expectedCommand);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Commands_can_have_aliases(bool useLazy)
        {
            Command command = CreateCommand(useLazy, "this", static () => new Command("this"));
            command.Aliases.Add("that");
            command.Name.Should().Be("this");
            command.Aliases.Should().BeEquivalentTo("that");
            command.Aliases.Should().BeEquivalentTo("that");

            ParseResult result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RootCommand_can_have_aliases(bool useLazy)
        {
            RootCommand command = new();
            command.Aliases.Add("that");
            command.Aliases.Should().BeEquivalentTo("that");
            command.Aliases.Should().BeEquivalentTo("that");
            if (useLazy)
            {
                // The action makes the subcommand optional, so parsing succeeds without creating it.
                command.SetAction(static _ => { });
                command.Add("sub", static () => throw new InvalidOperationException("sub should not be created"));
            }

            ParseResult result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Subcommands_can_have_aliases(bool useLazy)
        {
            RootCommand rootCommand = new();
            if (useLazy)
            {
                rootCommand.Add("this", ["that"], static () => new Command("this"));
            }
            else
            {
                Command subcommand = new("this");
                subcommand.Aliases.Add("that");
                rootCommand.Add(subcommand);
            }

            ParseResult result = rootCommand.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(rootCommand.Subcommands.Single());
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_retains_argument_name_when_it_is_provided(bool useLazy)
        {
            Command command = CreateCommand(useLazy, "-alias", static () => new Command("-alias")
            {
                new Argument<bool>("arg")
            });

            command.Arguments.Single().Name.Should().Be("arg");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddGlobalOption_updates_Options_property(bool useLazy)
        {
            Option<string> option = new("-x") { Recursive = true };
            Command command = CreateCommand(useLazy, "mycommand", static () => new Command("mycommand"));
            command.Options.Add(option);

            command.Options
                   .Should()
                   .Contain(option);
        }

        // https://github.com/dotnet/command-line-api/issues/1437
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void When_Options_is_referenced_before_a_global_option_is_added_then_adding_a_global_option_updates_the_Options_collection(bool useLazy)
        {
            Option<string> option = new("-x");
            Command command = CreateCommand(useLazy, "mycommand", static () => new Command("mycommand"));

            // referencing command.Options here would reproduce the above bug before the fix
            // keeping it ensures the fix works and doesn't regress
            command.Options
                .Should()
                .BeEmpty();

            option.Recursive = true;
            command.Options.Add(option);

            command.Options
                .Should()
                .Contain(option);
        }

        [Fact]
        public void Adding_lazy_subcommands_does_not_create_them()
        {
            RootCommand root = new();
            root.Add("a", static () => throw new InvalidOperationException("a should not be created"));
            root.Add("b", ["bb"], static () => throw new InvalidOperationException("b should not be created"));

            root.Subcommands.Count.Should().Be(2);
        }

        [Fact]
        public void Parser_creates_only_the_lazy_subcommands_that_were_matched()
        {
            List<string> created = new();
            RootCommand root = new();
            root.Add("a", () =>
            {
                created.Add("a");
                Command a = new("a");
                a.Add("b", () =>
                {
                    created.Add("b");
                    return new Command("b") { new Option<string>("--option") };
                });
                a.Add("c", static () => throw new InvalidOperationException("c should not be created"));
                return a;
            });
            root.Add("d", static () => throw new InvalidOperationException("d should not be created"));

            ParseResult result = root.Parse("a b --option value");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("b");
            result.GetValue<string>("--option").Should().Be("value");
            created.Should().Equal("a", "b");
        }

        [Fact]
        public void Lazy_subcommand_factory_is_invoked_only_once()
        {
            int factoryCalls = 0;
            RootCommand root = new();
            root.Add("a", () =>
            {
                factoryCalls++;
                return new Command("a");
            });

            ParseResult first = root.Parse("a");
            ParseResult second = root.Parse("a");

            second.CommandResult.Command.Should().BeSameAs(first.CommandResult.Command);
            root.Subcommands.Single().Should().BeSameAs(first.CommandResult.Command);
            factoryCalls.Should().Be(1);
        }

        [Fact]
        public void Lazy_subcommand_can_be_matched_by_alias_and_gets_the_declared_aliases()
        {
            RootCommand root = new();
            root.Add("solution", ["sln"], static () => new Command("solution"));
            root.Add("other", static () => throw new InvalidOperationException("other should not be created"));

            ParseResult result = root.Parse("sln");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("solution");
            result.CommandResult.Command.Aliases.Should().Equal("sln");
            result.CommandResult.Command.Parents.Should().Equal(root);
        }

        [Fact]
        public void Parse_errors_do_not_create_unmatched_lazy_subcommands()
        {
            RootCommand root = new();
            root.Add("a", static () => throw new InvalidOperationException("a should not be created"));

            ParseResult result = root.Parse("unknown");

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void Lazy_subcommand_factory_returning_command_with_different_name_throws()
        {
            RootCommand root = new();
            root.Add("a", static () => new Command("b"));

            Action parse = () => root.Parse("a");

            parse.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Lazy_subcommand_factory_cannot_be_null()
        {
            RootCommand root = new();

            Action add = () => root.Add("a", null!);

            add.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Lazy_subcommands_are_created_on_access_but_not_on_count()
        {
            int factoryCalls = 0;
            Command eager = new("eager");
            RootCommand root = new() { eager };
            root.Add("lazy", () =>
            {
                factoryCalls++;
                return new Command("lazy");
            });

            root.Subcommands.Count.Should().Be(2);
            root.Subcommands.Contains(eager).Should().BeTrue();
            factoryCalls.Should().Be(0);

            root.Subcommands.Select(c => c.Name).Should().Equal("eager", "lazy");
            root.Subcommands[1].Should().BeSameAs(root.Subcommands.Single(c => c.Name == "lazy"));
            factoryCalls.Should().Be(1);
        }

        [Fact]
        public void Help_lists_lazy_subcommands_without_creating_their_subcommands()
        {
            RootCommand root = new();
            root.Add("a", static () =>
            {
                Command a = new("a", "description of a");
                a.Add("b", static () => throw new InvalidOperationException("b should not be created"));
                return a;
            });
            StringWriter output = new();

            root.Parse("--help").Invoke(new() { Output = output });

            output.ToString().Should().Contain("description of a");
        }

        [Fact]
        public void Help_for_lazy_subcommand_does_not_create_its_siblings()
        {
            RootCommand root = new();
            root.Add("a", static () => new Command("a", "description of a"));
            root.Add("b", static () => throw new InvalidOperationException("b should not be created"));
            StringWriter output = new();

            root.Parse("a --help").Invoke(new() { Output = output });

            output.ToString().Should().Contain("description of a");
        }
    }
}
