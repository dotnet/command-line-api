// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Shells;

public class ShellProviderEscapingTests(ITestOutputHelper log)
{
    public static TheoryData<IShellProvider> Providers =>
        new(CompletionsCommandParser.ShellProviders.Values.ToArray());

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task EscapesStaticCompletionData(IShellProvider provider)
    {
        CompletionItem[] completions =
        [
            new("value with spaces"),
            new("single'quote"),
            new("double\"quote"),
            new(@"back\slash"),
            new("$variable"),
            new("$(command)"),
            new("`command`"),
            new("semi;pipe|amp&parens()brackets[]colon:glob*?"),
            new(
                label: "display ' \" $ ` \\",
                insertText: "insert ' \" $ ` \\",
                documentation: "documentation ' \" $ ` \\\nsecond line")
        ];

        var option = new Option<string>("--option");
        option.CompletionSources.Add(_ => completions);

        var argument = new Argument<string>("argument");
        argument.CompletionSources.Add(_ => completions);

        await provider.Verify(new Command("mycommand") { option, argument }, log);
    }
}
