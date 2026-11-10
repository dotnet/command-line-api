// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions;

public static class ShellNames
{
    public const string Bash = "bash";
    public const string PowerShell = "pwsh";
    public const string Fish = "fish";
    public const string Zsh = "zsh";
    public const string Nushell = "nushell";

    public static readonly IEnumerable<string> All =
    [
        Bash,
        PowerShell,
        Fish,
        Zsh,
        Nushell,
    ];

    public static string GetShellNameFromEnvironment()
    {
        if (OperatingSystem.IsWindows())
        {
            return PowerShell;
        }

        var shellPath = Environment.GetEnvironmentVariable("SHELL") ?? throw new InvalidOperationException(Strings.ShellDiscovery_ShellEnvironmentNotSet);

        var shellName = Path.GetFileName(shellPath);
        if (All.Contains(shellName))
        {
            return shellName;
        }

        throw new InvalidOperationException(string.Format(Strings.ShellDiscovery_ShellNotSupported, shellName, string.Join(", ", All)));
    }
}

