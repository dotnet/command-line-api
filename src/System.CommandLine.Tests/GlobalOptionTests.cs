// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class GlobalOptionTests
    {
        [Fact]
        public void Global_options_may_be_added_with_aliases_that_conflict_with_local_options()
        {
            var command = new Command("the-command")
            {
                new Option("--same")
            };

            command
                .Invoking(c => c.AddGlobalOption(new Option("--same")))
                .Should()
                .NotThrow<ArgumentException>();
        }

        [Fact]
        public void Global_options_may_not_have_aliases_conflicting_with_other_global_option_aliases()
        {
            var command = new Command("the-command");

            command.AddGlobalOption(new Option("--same"));

            command
                .Invoking(c => c.AddGlobalOption(new Option("--same")))
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Be("Alias '--same' is already in use.");
        }

        [Fact]
        public void When_local_options_are_added_then_they_must_differ_from_global_options_by_name()
        {
            var command = new Command("the-command");

            command.AddGlobalOption(new Option("--same"));

            command
                .Invoking(c => c.Add(new Option("--same")))
                .Should()
                .Throw<ArgumentException>()
                .And
                .Message
                .Should()
                .Be("Alias '--same' is already in use.");
        }

        [Fact]
        public void Global_options_appear_in_options_list()
        {
            var root = new Command("parent");

            var option = new Option<int>("--global");

            root.AddGlobalOption(option);

            var child = new Command("child");

            root.AddCommand(child);

            root.Options.Should().Contain(option);
        }

        [Fact]
        public void Subcommands_added_after_a_global_option_is_added_to_parent_will_recognize_the_global_option()
        {
            var root = new Command("parent");

            var option = new Option<int>("--global");

            root.AddGlobalOption(option);

            var child = new Command("child");

            root.AddCommand(child);

            root.Parse("child --global 123").ValueForOption(option).Should().Be(123);

            child.Parse("--global 123").ValueForOption(option).Should().Be(123);
        }

        [Fact]
        public void Subcommands_with_global_option_should_propagate_option_to_children()
        {
            var root = new Command("parent");
            
            var firstChild = new Command("first");
            
            root.AddCommand(firstChild);
            
            var option = new Option<int>("--global");
            
            firstChild.AddGlobalOption(option);
            
            var secondChild = new Command("second");
            
            firstChild.AddCommand(secondChild);
            
            root.Parse("first second --global 123").ValueForOption(option).Should().Be(123);
            
            firstChild.Parse("second --global 123").ValueForOption(option).Should().Be(123);
            
            secondChild.Parse("--global 123").ValueForOption(option).Should().Be(123);
        }
    }
}