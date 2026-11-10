// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.StaticCompletions.Shells;
using System.Runtime.CompilerServices;

namespace System.CommandLine.StaticCompletions.Tests;

public static class VerifyExtensions
{
    public static async Task Verify(this IShellProvider provider, Command command, ITestOutputHelper log, [CallerFilePath] string sourceFile = "")
    {
        var settings = new VerifySettings();
        settings.UseDirectory(Path.Combine("snapshots", provider.ArgumentName));
        var completions = provider.GenerateCompletions(command);
        await Verifier.Verify(target: completions, extension: provider.Extension, settings: settings, sourceFile: sourceFile);
    }
}
