// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions;

public sealed class CompletionsGenerateScriptCommandDefinition : Command
{
    public readonly Argument<string> ShellArgument;

    public CompletionsGenerateScriptCommandDefinition(CompletionsCommandDefinition parent)
       : base("script", Strings.GenerateCommand_Description)
    {
        Arguments.Add(ShellArgument = parent.ShellArgument);
    }
}

