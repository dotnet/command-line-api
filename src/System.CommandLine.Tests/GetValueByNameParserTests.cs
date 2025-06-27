// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace System.CommandLine.Tests;

public class GetValueByNameTests
{
    [Fact]
    public void In_case_of_argument_name_conflict_the_value_which_belongs_to_the_last_parsed_command_is_returned()
    {
        RootCommand command = new()
        {
            new Argument<int>("arg"),
            new Command("inner1")
            {
                new Argument<int>("arg"),
                new Command("inner2")
                {
                    new Argument<int>("arg"),
                }
            }
        };

        ParseResult parseResult = command.Parse("1 inner1 2 inner2 3");

        parseResult.GetValue<int>("arg").Should().Be(3);
    }

    [Fact]
    public void In_case_of_option_name_conflict_the_value_which_belongs_to_the_last_parsed_command_is_returned()
    {
        RootCommand command = new()
        {
            new Option<int>("--integer", "-i"),
            new Command("inner1")
            {
                new Option<int>("--integer", "-i"),
                new Command("inner2")
                {
                    new Option<int>("--integer", "-i")
                }
            }
        };

        ParseResult parseResult = command.Parse("-i 1 inner1 --integer 2 inner2 -i 3");

        parseResult.GetValue<int>("--integer").Should().Be(3);
    }

    [Fact]
    public void When_option_is_not_provided_then_default_value_is_returned()
    {
        RootCommand command = new()
        {
            new Option<int>("--integer", "-i")
        };

        ParseResult parseResult = command.Parse("");

        parseResult.GetValue<int>("--integer").Should().Be(default);
    }

    [Fact]
    public void When_option_is_not_provided_then_configured_default_value_is_returned()
    {
        RootCommand command = new()
        {
            new Option<int>("--integer", "-i")
            {
                DefaultValueFactory = _ => 123
            }
        };

        ParseResult parseResult = command.Parse("");

        parseResult.GetValue<int>("--integer").Should().Be(123);
    }

    [Fact]
    public void When_optional_argument_is_not_provided_then_default_value_is_returned()
    {
        RootCommand command = new()
        {
            new Argument<int>("arg")
            {
                Arity = ArgumentArity.ZeroOrOne
            }
        };

        ParseResult parseResult = command.Parse("");

        parseResult.GetValue<int>("arg").Should().Be(default);
    }

    [Fact]
    public void When_optional_argument_is_not_provided_then_configured_default_value_is_returned()
    {
        RootCommand command = new()
        {
            new Argument<int>("arg")
            {
                Arity = ArgumentArity.ZeroOrOne,
                DefaultValueFactory = _ => 123
            }
        };
        
        ParseResult parseResult = command.Parse("");

        parseResult.GetValue<int>("arg").Should().Be(123);
    }

    [Fact]
    public void When_required_option_value_is_not_provided_then_an_exception_is_thrown()
    {
        RootCommand command = new()
        {
            new Option<int>("--required")
            {
                Required = true
            }
        };

        ParseResult parseResult = command.Parse("");

        Action getRequired = () => parseResult.GetValue<int>("--required");

        getRequired
            .Should()
            .Throw<InvalidOperationException>()
            .Where(ex => ex.Message == LocalizationResources.RequiredOptionWasNotProvided("--required"));
    }

    [Fact]
    public void When_required_argument_value_is_not_provided_then_an_exception_is_thrown()
    {
        RootCommand command = new()
        {
            new Argument<int>("required")
            {
                Arity = ArgumentArity.ExactlyOne
            }
        };

        ParseResult parseResult = command.Parse("");

        Action getRequired = () => parseResult.GetValue<int>("required");

        getRequired
            .Should()
            .Throw<InvalidOperationException>()
            .Where(ex => ex.Message == LocalizationResources.RequiredArgumentMissing(parseResult.GetResult(command.Arguments[0])));
    }

    [Fact]
    public void When_non_existing_name_is_used_then_exception_is_thrown()
    {
        const string nonExistingName = "nonExisting";
        Command command = new ("noSymbols");
        ParseResult parseResult = command.Parse("");

        Action getRequired = () => parseResult.GetValue<int>(nonExistingName);

        getRequired
            .Should()
            .Throw<ArgumentException>()
            .Where(ex => ex.Message == $"No symbol result found with name \"{nonExistingName}\".");
    }

    [Fact]
    public void When_an_option_and_argument_use_same_name_on_the_same_level_of_the_tree_an_exception_is_thrown()
    {
        const string sameName = "same";

        RootCommand command = new()
        {
            new Argument<int>(sameName)
            {
                Arity = ArgumentArity.ZeroOrOne
            },
            new Option<int>(sameName)
        };

        ParseResult parseResult = command.Parse("");

        Action getConflicted = () => parseResult.GetValue<int>(sameName);

        getConflicted
            .Should()
            .Throw<InvalidOperationException>()
            .Where(ex => ex.Message == $"Command {command.Name} has more than one child named \"{sameName}\".");
    }

    [Fact]
    public void When_options_use_same_name_on_different_levels_of_the_tree_no_exception_is_thrown()
    {
        const string sameName = "same";

        RootCommand command = new()
        {
            new Command("left")
            {
                new Option<int>(sameName)
            },
            new Command("right")
            {
                new Option<int>(sameName)
            },
        };

        command.Parse($"left {sameName} 1").GetValue<int>(sameName).Should().Be(1);
        command.Parse($"right {sameName} 2").GetValue<int>(sameName).Should().Be(2);
    }

    [Fact]
    public void When_the_same_option_used_in_different_levels_of_the_tree_no_exception_is_thrown()
    {
        Option<int> multipleParents = new("--int");

        RootCommand command = new()
        {
            new Command("left")
            {
                multipleParents
            },
            new Command("right")
            {
                multipleParents
            },
        };

        command.Parse($"left {multipleParents.Name} 1").GetValue<int>(multipleParents.Name).Should().Be(1);
        command.Parse($"right {multipleParents.Name} 2").GetValue<int>(multipleParents.Name).Should().Be(2);
    }

    [Fact]
    public void When_an_option_and_argument_use_same_name_on_different_levels_of_the_tree_the_value_which_belongs_to_parsed_command_is_returned()
    {
        const string sameName = "same";

        Command command = new("outer")
        {
            new Argument<int>(sameName),
            new Command("inner")
            {
                new Option<int>(sameName)
            }
        };

        ParseResult parseResult = command.Parse($"outer 123 inner {sameName} 456");
        parseResult.GetValue<int>(sameName).Should().Be(456);

        parseResult = command.Parse("outer 123");
        parseResult.GetValue<int>(sameName).Should().Be(123);
    }

    [Fact]
    public void When_an_option_and_argument_use_same_name_on_different_levels_of_the_tree_the_default_value_which_belongs_to_parsed_command_is_returned()
    {
        const string sameName = "same";

        Command command = new("outer")
        {
            new Argument<int>(sameName)
            {
                DefaultValueFactory = _ => 123
            },
            new Command("inner")
            {
                new Option<int>(sameName)
                {
                    DefaultValueFactory = _ => 456
                }
            }
        };

        ParseResult parseResult = command.Parse("outer inner 456");
        parseResult.GetValue<int>(sameName).Should().Be(456);

        parseResult = command.Parse("outer 123");
        parseResult.GetValue<int>(sameName).Should().Be(123);
    }

    [Fact]
    public void T_can_be_cast_to_nullable_of_T()
    {
        RootCommand command = new()
        {
            new Argument<int>("name")
        };

        ParseResult parseResult = command.Parse("123");

        parseResult.GetValue<int?>("name").Should().Be(123);
    }

    [Fact]
    public void Array_of_T_can_be_cast_to_IEnumerable_of_T()
    {
        RootCommand command = new()
        {
            new Argument<int[]>("name")
        };

        ParseResult parseResult = command.Parse("1 2 3");

        parseResult.GetValue<IEnumerable<int>>("name").Should().BeEquivalentTo(new int[] { 1, 2, 3 });
    }

    [Fact]
    public void When_cast_is_invalid_then_an_exception_is_thrown()
    {
        const string Name = "name";

        RootCommand command = new()
        {
            new Argument<int>(Name)
        };

        ParseResult parseResult = command.Parse("123");

        Assert(() => parseResult.GetValue<double>(Name));
        Assert(() => parseResult.GetValue<int[]>(Name));
        Assert(() => parseResult.GetValue<string>(Name));

        static void Assert(Action invalidCast)
            => invalidCast.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void Parse_errors_have_precedence_over_type_mismatch()
    {
        RootCommand command = new()
        {
            new Option<int>("--required")
            {
                Required = true
            }
        };

        ParseResult parseResult = command.Parse("");

        Action getRequiredWithTypeMismatch = () => parseResult.GetValue<double>("--required");

        getRequiredWithTypeMismatch
            .Should()
            .Throw<InvalidOperationException>()
            .Where(ex => ex.Message == LocalizationResources.RequiredOptionWasNotProvided("--required"));
    }

    [Fact]
    public void Recursive_option_on_parent_command_can_be_looked_up_when_subcommand_is_specified()
    {
        var cmd = new RootCommand
        {
            new Command("subcommand"),

            new Option<string>("--opt")
            {
                Recursive = true
            }
        };

        var result = cmd.Parse("subcommand --opt hello");

        result.GetValue<string>("--opt").Should().Be("hello");
    }

    [Fact]
    public void When_argument_type_is_unknown_then_named_lookup_can_be_used_to_get_value_as_supertype()
    {
        var command = new RootCommand
        {
            new Argument<string>("arg")
        };

        var result = command.Parse("value");

        result.GetValue<object>("arg").Should().Be("value");
    }

    [Fact]
    public void When_option_type_is_unknown_then_named_lookup_can_be_used_to_get_value_as_supertype()
    {
        var command = new RootCommand
        {
            new Option<string>("-x")
        };

        var result = command.Parse("-x value");

        result.GetValue<object>("-x").Should().Be("value");
    }
}