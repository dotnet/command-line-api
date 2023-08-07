// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class GlobalOptionTests
    {
        [Fact]
        public void Global_options_appear_in_options_list_of_symbols_they_are_directly_added_to()
        {
            var root = new CliCommand("parent");

            var option = new CliOption<int>("--global") { Recursive = true };

            root.Options.Add(option);

            root.Options.Should().Contain(option);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1540
        public void When_a_required_global_option_is_omitted_it_results_in_an_error()
        {
            var command = new CliCommand("child");
            var rootCommand = new CliRootCommand { command };
            command.SetAction((_) => { });
            var requiredOption = new CliOption<bool>("--i-must-be-set")
            {
                Required = true,
                Recursive = true
            };
            rootCommand.Options.Add(requiredOption);

            var result = rootCommand.Parse("child");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which.Message.Should().Be("Option '--i-must-be-set' is required.");
        }

        [Fact] 
        public void When_a_required_global_option_has_multiple_aliases_the_error_message_uses_the_name()
        {
            var rootCommand = new CliRootCommand();
            var requiredOption = new CliOption<bool>("-i", "--i-must-be-set")
            {
                Required = true,
                Recursive = true
            };
            rootCommand.Options.Add(requiredOption);

            var result = rootCommand.Parse("");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which.Message.Should().Be("Option '-i' is required.");
        }

        [Fact]
        public void When_a_required_global_option_is_present_on_child_of_command_it_was_added_to_it_does_not_result_in_an_error()
        {
            var command = new CliCommand("child");
            var rootCommand = new CliRootCommand { command };
            command.SetAction((_) => { });
            var requiredOption = new CliOption<bool>("--i-must-be-set")
            {
                Required = true,
                Recursive = true
            };
            rootCommand.Options.Add(requiredOption);

            var result = rootCommand.Parse("child --i-must-be-set");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Subcommands_added_after_a_global_option_is_added_to_parent_will_recognize_the_global_option()
        {
            var root = new CliCommand("parent");

            var option = new CliOption<int>("--global") { Recursive = true };

            root.Options.Add(option);

            var child = new CliCommand("child");

            root.Subcommands.Add(child);

            root.Parse("child --global 123").GetValue(option).Should().Be(123);

            child.Parse("--global 123").GetValue(option).Should().Be(123);
        }

        [Fact]
        public void Subcommands_with_global_option_should_propagate_option_to_children()
        {
            var root = new CliCommand("parent");
            
            var firstChild = new CliCommand("first");
            
            root.Subcommands.Add(firstChild);
            
            var option = new CliOption<int>("--global") { Recursive = true };
            
            firstChild.Options.Add(option);
            
            var secondChild = new CliCommand("second");
            
            firstChild.Subcommands.Add(secondChild);
            
            root.Parse("first second --global 123").GetValue(option).Should().Be(123);
            
            firstChild.Parse("second --global 123").GetValue(option).Should().Be(123);
            
            secondChild.Parse("--global 123").GetValue(option).Should().Be(123);
        }
    }
}