﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class GlobalOptionTests
    {
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

        [Fact] // https://github.com/dotnet/command-line-api/issues/1540
        public void When_a_required_global_option_is_omitted_it_results_in_an_error()
        {
            var command = new Command("child");
            var rootCommand = new RootCommand { command };
            command.SetHandler(() => { });
            var requiredOption = new Option<bool>("--i-must-be-set")
            {
                IsRequired = true
            };
            rootCommand.AddGlobalOption(requiredOption);

            var result = rootCommand.Parse("child");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which.Message.Should().Be("Option '--i-must-be-set' is required.");
        }

        [Fact] 
        public void When_a_required_global_option_has_multiple_aliases_the_error_message_uses_longest()
        {
            var rootCommand = new RootCommand();
            var requiredOption = new Option<bool>(new[] { "-i", "--i-must-be-set" })
            {
                IsRequired = true
            };
            rootCommand.AddGlobalOption(requiredOption);

            var result = rootCommand.Parse("");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which.Message.Should().Be("Option '--i-must-be-set' is required.");
        }

        [Fact]
        public void When_a_required_global_option_is_present_on_child_of_command_it_was_added_to_it_does_not_result_in_an_error()
        {
            var command = new Command("child");
            var rootCommand = new RootCommand { command };
            command.SetHandler(() => { });
            var requiredOption = new Option<bool>("--i-must-be-set")
            {
                IsRequired = true
            };
            rootCommand.AddGlobalOption(requiredOption);

            var result = rootCommand.Parse("child --i-must-be-set");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Subcommands_added_after_a_global_option_is_added_to_parent_will_recognize_the_global_option()
        {
            var root = new Command("parent");

            var option = new Option<int>("--global");

            root.AddGlobalOption(option);

            var child = new Command("child");

            root.AddCommand(child);

            root.Parse("child --global 123").GetValue(option).Should().Be(123);

            child.Parse("--global 123").GetValue(option).Should().Be(123);
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
            
            root.Parse("first second --global 123").GetValue(option).Should().Be(123);
            
            firstChild.Parse("second --global 123").GetValue(option).Should().Be(123);
            
            secondChild.Parse("--global 123").GetValue(option).Should().Be(123);
        }
    }
}