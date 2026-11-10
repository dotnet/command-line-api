// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions.Shells;

public class NushellShellProvider : IShellProvider
{
    public string ArgumentName => ShellNames.Nushell;

    public string Extension => "nu";

    public string HelpDescription => Strings.NuShellShellProvider_HelpDescription;

    // override the ToString method to return the argument name so that CLI help is cleaner for 'default' values
    public override string ToString() => ArgumentName;

    public string GenerateCompletions(System.CommandLine.Command command)
    {
        var binary = command.Name;
        return
            $$"""
            # nushell completions for {{binary}}
            # save this file and `source` it from your nushell config

            def "nu-complete {{binary}}" [context: string] {
                ^{{binary}} $"[suggest:($context | str length)]" $context | lines
            }

            export extern "{{binary}}" [
                ...command: string@"nu-complete {{binary}}"
            ]
            """;
    }
}
