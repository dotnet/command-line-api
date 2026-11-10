// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions;

public sealed class CompletionsCommandDefinition : Command
{
    public readonly Argument<string> ShellArgument = new("shell")
    {
        Description = Strings.CompletionsCommand_ShellArgument_Description,
        Arity = ArgumentArity.ZeroOrOne,
        DefaultValueFactory = _ => ShellNames.GetShellNameFromEnvironment()
    };

    public readonly CompletionsGenerateScriptCommandDefinition GenerateScriptCommand;

    public CompletionsCommandDefinition()
        : base("completions", Strings.CompletionsCommand_Description)
    {
        Subcommands.Add(GenerateScriptCommand = new(this));

        Validators.Add(argumentResult =>
        {
            if (argumentResult.Tokens.Count == 0)
            {
                return;
            }

            var singleToken = argumentResult.Tokens[0];
            if (!ShellNames.All.Contains(singleToken.Value))
            {
                argumentResult.AddError(string.Format(Strings.ShellDiscovery_ShellNotSupported, singleToken.Value, string.Join(", ", ShellNames.All)));
            }
        });
    }
}
