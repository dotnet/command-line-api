﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class TokenReplacementTests
{
    [Fact]
    public void Token_replacer_receives_the_token_from_the_command_line_with_the_leading_at_symbol_removed()
    {
        var argument = new Argument<int>();

        var command = new RootCommand { argument };

        string receivedToken = null;

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         receivedToken = tokenToReplace;
                         tokens = null;
                         message = "oops!";
                         return false;
                     })
                     .Build();

        command.Parse("@replace-me", config);

        receivedToken.Should().Be("replace-me");
    }

    [Fact]
    public void Token_replacer_can_expand_argument_values()
    {
        var argument = new Argument<int>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = new[] { "123" };
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(argument).Should().Be(123);
    }

    [Fact]
    public void Custom_token_replacer_can_expand_option_argument_values()
    {
        var option = new Option<int>("-x");

        var command = new RootCommand { option };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = new[] { "123" };
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("-x @replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(option).Should().Be(123);
    }

    [Fact]
    public void Custom_token_replacer_can_expand_subcommands_and_options_and_argument()
    {
        var option = new Option<int>("-x");

        var command = new RootCommand { new Command("subcommand") { option } };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = new[] { "subcommand", "-x", "123" };
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(option).Should().Be(123);
    }

    [Fact]
    public void Expanded_tokens_containing_whitespace_are_parsed_as_single_tokens()
    {
        var argument = new Argument<string>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = new[] { "one two three" };
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.GetValue(argument).Should().Be("one two three");
    }

    [Fact]
    public void Token_replacer_can_set_a_custom_error_message()
    {
        var argument = new Argument<string>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = null;
                         message = "oops!";
                         return false;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors
              .Should()
              .ContainSingle(e => e.Message == "oops!");
    }
    
    [Fact]
    public void When_token_replacer_returns_false_without_setting_an_error_message_then_the_command_line_is_unchanged_and_no_parse_error_is_produced()
    {
        var argument = new Argument<string>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = null;
                         message = null;
                         return false;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(argument).Should().Be("@replace-me");
    }

    [Fact]
    public void Token_replacer_will_delete_token_when_delegate_returns_true_and_sets_tokens_to_null()
    {
        var argument = new Argument<string[]>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = null;
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(argument).Should().BeEmpty();
    }

    [Fact]
    public void Token_replacer_will_delete_token_when_delegate_returns_true_and_sets_tokens_to_empty_array()
    {
        var argument = new Argument<string[]>();

        var command = new RootCommand { argument };

        var config = new CommandLineBuilder(command)
                     .UseTokenReplacer((string tokenToReplace, out IReadOnlyList<string> tokens, out string message) =>
                     {
                         tokens = Array.Empty<string>();
                         message = null;
                         return true;
                     })
                     .Build();

        var result = command.Parse("@replace-me", config);

        result.Errors.Should().BeEmpty();

        result.GetValue(argument).Should().BeEmpty();
    }
}