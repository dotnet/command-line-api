// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.Completions;

public class CompletionsCommandParserTests
{
    private static RootCommand CreateRootCommandWithCompletions(bool withDynamicSymbol)
    {
        var rootCommand = new RootCommand("my test app");
        if (withDynamicSymbol)
        {
            rootCommand.Options.Add(new Option<string>("--name") { IsDynamic = true });
        }

        var completionsCommand = new CompletionsCommandDefinition();
        CompletionsCommandParser.ConfigureCommand(completionsCommand);
        rootCommand.Subcommands.Add(completionsCommand);
        return rootCommand;
    }

    private static (int exitCode, string output, string error) GenerateScript(RootCommand rootCommand)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = rootCommand.Parse(["completions", "script", "bash"]).Invoke(new InvocationConfiguration
        {
            Output = output,
            Error = error
        });
        return (exitCode, output.ToString(), error.ToString());
    }

    [Fact]
    public void NoWarningWhenSuggestDirectiveIsEnabled()
    {
        var rootCommand = CreateRootCommandWithCompletions(withDynamicSymbol: true);

        var (exitCode, output, error) = GenerateScript(rootCommand);

        exitCode.Should().Be(0);
        output.Should().NotBeEmpty();
        error.Should().BeEmpty();
    }

    [Fact]
    public void WarnsWhenSuggestDirectiveIsDisabledAndDynamicSymbolsArePresent()
    {
        var rootCommand = CreateRootCommandWithCompletions(withDynamicSymbol: true);
        rootCommand.Directives.Remove(rootCommand.Directives.OfType<SuggestDirective>().Single());

        var (exitCode, output, error) = GenerateScript(rootCommand);

        // the script is still generated, but the user is warned that dynamic completions won't resolve
        exitCode.Should().Be(0);
        output.Should().NotBeEmpty();
        error.Should().Contain("[suggest]");
    }

    [Fact]
    public void NoWarningWhenSuggestDirectiveIsDisabledButNoDynamicSymbolsArePresent()
    {
        var rootCommand = CreateRootCommandWithCompletions(withDynamicSymbol: false);
        rootCommand.Directives.Remove(rootCommand.Directives.OfType<SuggestDirective>().Single());

        var (exitCode, output, error) = GenerateScript(rootCommand);

        exitCode.Should().Be(0);
        output.Should().NotBeEmpty();
        error.Should().BeEmpty();
    }
}
