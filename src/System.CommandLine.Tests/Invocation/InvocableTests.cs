// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation;

public class InvocableTests
{
    private readonly TestConsole _console = new();

    private static RootCommand CreateRootCommand()
    {
        var subcommandOne = new Command("one");

        var root = new RootCommand
        {
            subcommandOne
        };

        subcommandOne.SetHandler(CliAction.Create(ctx => ctx.Console.Write("hello from one")));
        root.SetHandler(CliAction.Create(ctx => ctx.Console.Write("hello from root")));

        return root;
    }

    [Fact]
    public async Task root_command_handler_base_case()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Be("hello from root");
    }

    [Fact]
    public async Task subcommand_handler_base_case()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Be("hello from one");
    }

    [Fact]
    public async Task root_command_help_option_base_case()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("-h");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Match($"*Usage:*{RootCommand.ExecutableName}*");
    }

    [Fact]
    public async Task subcommand_help_option_base_case()
    {
        var root = CreateRootCommand();

        var parseResult = root.Parse("one -h");

        await parseResult.Action.RunAsync(_console);

        _console.Out.ToString().Should().Match("*Usage:*one*");
    }
}