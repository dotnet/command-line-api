// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class AliasedSetTests
    {
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
                .Contains("after")
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
                .Contains("before")
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
    }
}