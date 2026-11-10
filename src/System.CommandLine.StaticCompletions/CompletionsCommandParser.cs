// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Shells;
using System.Diagnostics;

namespace System.CommandLine.StaticCompletions;

public sealed class CompletionsCommandParser
{
    public static readonly IReadOnlyDictionary<string, IShellProvider> ShellProviders;

    static CompletionsCommandParser()
    {
        var providers = new IShellProvider[]
        {
            new BashShellProvider(),
            new PowerShellShellProvider(),
            new FishShellProvider(),
            new ZshShellProvider(),
            new NushellShellProvider()
        };

        Debug.Assert(providers.Select(provider => provider.ArgumentName).SequenceEqual(ShellNames.All));

        ShellProviders = providers.ToDictionary(s => s.ArgumentName, StringComparer.OrdinalIgnoreCase);
    }

    public static void ConfigureCommand(CompletionsCommandDefinition command)
    {
        command.ShellArgument.CompletionSources.Add(context =>
            ShellNames.All.Select(shellName => new CompletionItem(shellName, documentation: ShellProviders[shellName].HelpDescription)));

        command.GenerateScriptCommand.SetAction(args =>
        {
            var shellName = args.GetValue(command.GenerateScriptCommand.ShellArgument) ?? throw new InvalidOperationException();
            var shell = ShellProviders[shellName];

            var rootCommand = args.RootCommandResult.Command;

            // the generated scripts resolve dynamic completions by calling back into the application
            // via the [suggest] directive - if that directive has been removed from the root command,
            // those parts of the script will silently do nothing, so let the user know at generation time.
            if (HasDynamicSymbols(rootCommand) && !HasSuggestDirective(rootCommand))
            {
                args.InvocationConfiguration.Error.WriteLine(Resources.Strings.GenerateCommand_SuggestDirectiveDisabledWarning);
            }

            var script = shell.GenerateCompletions(rootCommand);
            args.InvocationConfiguration.Output.Write(script);
        });
    }

    private static bool HasSuggestDirective(Command command) =>
        command is RootCommand root && root.Directives.OfType<SuggestDirective>().Any();

    private static bool HasDynamicSymbols(Command command) =>
        command.Options.Any(o => !o.Hidden && o.IsDynamic) ||
        command.Arguments.Any(a => !a.Hidden && a.IsDynamic) ||
        command.Subcommands.Where(c => !c.Hidden).Any(HasDynamicSymbols);
}
