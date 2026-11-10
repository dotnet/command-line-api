// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.StaticCompletions.Shells;

public class FishShellProviderTests(ITestOutputHelper log)
{
    private IShellProvider provider = new FishShellProvider();

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
        var dynamicOption = new Option<string>("--name") { IsDynamic = true };
        var dynamicArg = new Argument<string>("target") { IsDynamic = true };
        await provider.Verify(new("mycommand") { dynamicOption, dynamicArg }, log);
    }

    [Fact]
    public async Task StaticOptionValues()
    {
        var staticOption = new Option<int>("--verbosity");
        staticOption.AcceptOnlyFromAmong("quiet", "minimal", "normal", "detailed", "diagnostic");
        await provider.Verify(new("mycommand") {
            staticOption
        }, log);
    }

    [Fact]
    public async Task BoundedMultiValueOption()
    {
        var multiOption = new Option<string[]>("--sources")
        {
            Arity = new ArgumentArity(1, 3)
        };
        multiOption.AcceptOnlyFromAmong("src1", "src2", "src3", "src4");
        await provider.Verify(new("mycommand") {
            multiOption,
            new Command("subcommand")
        }, log);
    }

    [Fact]
    public async Task UnboundedMultiValueOption()
    {
        var unboundedOption = new Option<string[]>("--items")
        {
            Arity = ArgumentArity.ZeroOrMore
        };
        unboundedOption.AcceptOnlyFromAmong("a", "b", "c");
        await provider.Verify(new("mycommand") {
            unboundedOption,
            new Option<string>("--name"),
            new Command("subcommand")
        }, log);
    }

    [Fact]
    public async Task MixedArityOptions()
    {
        var singleOption = new Option<string>("--config");
        singleOption.AcceptOnlyFromAmong("debug", "release");

        var multiOption = new Option<string[]>("--framework", "-f")
        {
            Arity = new ArgumentArity(1, 3)
        };
        multiOption.AcceptOnlyFromAmong("net8.0", "net9.0", "net10.0");

        var unboundedOption = new Option<string[]>("--sources")
        {
            Arity = ArgumentArity.OneOrMore
        };

        await provider.Verify(new("mycommand") {
            singleOption,
            multiOption,
            unboundedOption,
            new Command("build")
        }, log);
    }
}
