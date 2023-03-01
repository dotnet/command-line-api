// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation;

public class CliActionTests
{
    private readonly TestConsole _console = new();

    private static RootCommand CreateRootCommand()
    {
        var subcommandOne = new Command("one");

        var root = new RootCommand
        {
            subcommandOne
        };

        subcommandOne.SetAction(ctx => ctx.Console.Write("hello from one"));
        root.SetAction(ctx => ctx.Console.Write("hello from root"));

        return root;
    }

    [Fact]
    public async Task when_root_command_is_indicated_then_ParseResult_Action_RunAsync_calls_its_handler()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Be("hello from root");
    }

    [Fact]
    public async Task when_subcommand_is_indicated_then_ParseResult_Action_RunAsync_calls_its_handler()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Be("hello from one");
    }

    [Fact]
    public async Task when_root_command_help_is_indicated_then_ParseResult_Action_RunAsync_calls_its_handler()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("-h");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Match($"*Usage:*{RootCommand.ExecutableName}*");
    }

    [Fact]
    public async Task when_subcommand_help_is_indicated_then_ParseResult_Action_RunAsync_calls_its_handler()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one -h");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Match("*Usage:*one*");
    }

    [Fact]
    public void user_defined_types_can_be_used()
    {
        var commandOne = new Command("one");

        var commandTwo = new Command("two");

        var root = new RootCommand
        {
            commandOne,
            commandTwo
        };

        commandOne.SetAction(new CustomActionOne());
        commandTwo.SetAction(new CustomActionTwo());

        root.Parse("one").Action.Should().BeOfType<CustomActionOne>();
    }

    [Fact]
    public async Task instead_of_middleware_a_switch_statement_can_be_used_to_intercept_default_behaviors()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one -h");

        switch (parseResult.Action)
        {
            case HelpAction helpAction:
                _console.WriteLine("START");
                await helpAction.RunAsync(_console);
                _console.WriteLine("END");
                break;

            default:
                await parseResult.Action.RunAsync(_console);
                break;
        }

        _console.Out.ToString().Should().Match("START*Usage:*one*END*");
    }

    [Fact]
    public async Task instead_of_middleware_a_switch_statement_can_be_used_to_redirect_default_behaviors()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one");

        switch (parseResult.Action)
        {
            default:
                await new HelpAction(new HelpOption()).RunAsync(_console);

                await parseResult.Action.RunAsync(_console);

                break;
        }

        _console.Out.ToString().Should().Match("*Usage:*one*");
    }
}

public class CustomActionOne : CommandAction
{
}

public class CustomActionTwo : CommandAction
{
}