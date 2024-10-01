// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

// Notes from Chet's work on static shells and further thoughts
//    - Chet has work he can later upstream for completion script creation - static when it can be
//    - Completions need to know - what it is, whether it is static or dynamic, and how many items would be in the list.
//      - Not sure whether these need to be in the Completer or the trait
//    - Validation can have many validators per type. Completions may need to have a single one.
//    - Probably two steps - determining the available values and matching the current word
//      - The code in CompletionContext of main/Extended to get current word requires tokens and is pretty gnarly 
//    - File and directory are special as they can get handed off to shell ro the work

public class CompletionSubsystem : CliSubsystem
{
    public CompletionSubsystem()
        : base(CompletionAnnotations.Prefix, SubsystemKind.Completion)
    { }

    // TODO: Figure out trigger for completions
    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is null
            ? false
            : false;

    public override void Execute(PipelineResult pipelineResult)
    {
        pipelineResult.ConsoleHack.WriteLine("Not yet implemented");
        pipelineResult.SetSuccess();
    }
}
