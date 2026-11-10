// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.Help;
using System.CommandLine.StaticCompletions.Shells;

public class ZshShellProviderTests(ITestOutputHelper log)
{
    private IShellProvider _provider = new ZshShellProvider();

    [Fact]
    public async Task GenericCompletions()
    {
        Command command = new Command("my-app") {
            new Option<bool>("-c") {
                Arity = ArgumentArity.Zero,
                Recursive = true
            },
            new Option<bool>("-v") {
                Arity = ArgumentArity.Zero
            },
            new HelpOption(),
            new Command("test", "Subcommand\nwith a second line") {
                new Option<bool>("--debug", "-d")
                {
                    Arity = ArgumentArity.Zero
                }
            },
            new Command("help", "Print this message or the help of the given subcommand(s)") {
                new Command("test")
            }
        };
        await _provider.Verify(command, log);
    }

    [Fact]
    public async Task DynamicCompletionsGeneration()
    {
        var staticOption = new Option<int>("--static")
        {
            IsDynamic = true
        };
        staticOption.AcceptOnlyFromAmong("1", "2", "3");
        var dynamicArg = new Argument<int>("--dynamic")
        {
            IsDynamic = true
        };
        dynamicArg.CompletionSources.Add((context) =>
        {
            return [
                new ("4"),
                new ("5"),
                new ("6")
            ];
        });
        Command command = new Command("my-app")
        {
            staticOption,
            dynamicArg
        };
        await _provider.Verify(command, log);
    }

    [Fact]
    public async Task CustomStaticCompletionsGeneration()
    {
        var staticOption = new Option<int>("--static");
        staticOption.AcceptOnlyFromAmong("1", "2", "3");
        var dynamicArg = new Argument<int>("--dynamic");
        dynamicArg.CompletionSources.Add((context) =>
        {
            return [
                new ("4"),
                new ("5"),
                new ("6")
            ];
        });
        Command command = new Command("my-app")
        {
            staticOption,
            dynamicArg
        };
        await _provider.Verify(command, log);
    }

}
