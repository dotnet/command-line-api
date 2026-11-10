// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.StaticCompletions.Shells;
using EmptyFiles;

public class PowershellProviderTests(ITestOutputHelper log)
{
    private IShellProvider provider = new PowerShellShellProvider();

    [Fact]
    public async Task GenericCompletions()
    {
        await provider.Verify(new("mycommand"), log);
    }

    [Fact]
    public async Task SimpleOptionCompletion()
    {
        await provider.Verify(new("mycommand") {
            new Option<string>("--name")
        }, log);
    }

    [Fact]
    public async Task SubcommandAndOptionInTopLevelList()
    {
        await provider.Verify(new("mycommand") {
                new Option<string>("--name"),
                new Command("subcommand")
            }, log);
    }

    [Fact]
    public async Task NestedSubcommandCompletion()
    {
        await provider.Verify(new("mycommand") {
            new Command("subcommand") {
                new Command("nested")
            }
        }, log);
    }

    [Fact]
    public async Task DynamicCompletionsGeneration()
    {
        var dynamicArg = new Argument<string>("target") { IsDynamic = true };
        await provider.Verify(new("mycommand") { dynamicArg }, log);
    }
}
