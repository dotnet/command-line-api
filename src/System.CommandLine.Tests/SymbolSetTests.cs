// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolSetTests
    {
        [Fact]
        public void GetByAlias_returns_null_when_command_alias_is_not_found()
        {
            var set = CreateSet(new Command("x"));

            set.GetByAlias("y").Should().BeNull();
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_command_name_is_found()
        {
            var command = new Command("x");

            var set = CreateSet(command);

            set.GetByAlias("x").Should().NotBeNull();
        }

        [Fact]
        public void GetByAlias_returns_null_when_option_alias_is_not_found()
        {
            var set = CreateSet(new Option<string>("-x"));

            set.GetByAlias("-y").Should().BeNull();
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_option_alias_is_found()
        {
            var option = new Option<string>("-x");

            var set = CreateSet(option);

            set.GetByAlias("-x")
               .Should()
               .NotBeNull();
        }

        [Fact]
        public void GetByAlias_returns_null_when_argument_alias_is_not_found()
        {
            var set = CreateSet(new Argument<string>("x"));

            set.GetByAlias("y").Should().BeNull();
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_argument_alias_is_found()
        {
            var set = CreateSet(new Argument<string>("x"));

            set.GetByAlias("x").Should().NotBeNull();
        }

        [Fact]
        public void When_Name_is_changed_then_Contains_returns_true_for_new_name()
        {
            var command = new Command("before");

            var rootCommand = new RootCommand
            {
                command
            };

            command.Name = "after";

            rootCommand
                .Children
                .ContainsAlias("after")
                .Should()
                .BeTrue();
        }

        [Fact]
        public void When_Name_is_changed_then_Contains_returns_false_for_old_name()
        {
            var command = new Command("before");

            var rootCommand = new RootCommand
            {
                command
            };

            command.Name = "after";

            rootCommand
                .Children
                .ContainsAlias("before")
                .Should()
                .BeFalse();
        }

        [Fact]
        public void When_option_alias_is_changed_then_GetByAlias_returns_true_for_the_new_alias()
        {
            var symbol = new Option<string>("original");

            var command = new RootCommand
            {
                symbol
            };

            symbol.AddAlias("added");

            command.Children
                   .GetByAlias("added")
                   .Should()
                   .BeSameAs(symbol);
        }

        [Fact]
        public void When_command_alias_is_changed_then_GetByAlias_returns_true_for_the_new_alias()
        {
            var symbol = new Command("original");

            var command = new RootCommand
            {
                symbol
            };

            symbol.AddAlias("added");

            command.Children
                   .GetByAlias("added")
                   .Should()
                   .BeSameAs(symbol);
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_command_name_has_been_changed()
        {
            var command = new Command("old");

            var set = CreateSet(command);

            command.Name = "new";

            set.GetByAlias("new").Should().NotBeNull();
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_option_name_has_been_changed()
        {
            var option = new Option<string>("--old");

            var set = CreateSet(option);

            option.Name = "--new";

            set.GetByAlias("--new")
               .Should()
               .NotBeNull();
        }

        [Fact]
        public void GetByAlias_returns_expected_item_when_option_alias_has_been_added()
        {
            var option = new Option<string>("--old");

            var set = CreateSet(option);

            option.AddAlias("--new");

            set.GetByAlias("--new")
               .Should()
               .NotBeNull();
        }

        private SymbolSet CreateSet(Symbol symbol)
        {
            return new RootCommand
            {
                symbol
            }.Children;
        }
    }
}