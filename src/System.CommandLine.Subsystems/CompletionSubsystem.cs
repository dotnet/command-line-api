// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;

namespace System.CommandLine;

public class CompletionSubsystem : CliSubsystem
{
    public CompletionSubsystem(IAnnotationProvider? annotationProvider = null)
        : base("Completion", annotationProvider, SubsystemKind.Completion)
    { }

    // TODO: Figure out trigger for completions
    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is null
            ? false
            : false;

    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        pipelineContext.ConsoleHack.WriteLine("Not yet implemented");
        return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
    }
}
