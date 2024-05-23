// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public class CompletionSubsystem : CliSubsystem
{
    public CompletionSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(CompletionAnnotations.Prefix, SubsystemKind.Completion, annotationProvider)
    { }

    // TODO: Figure out trigger for completions
    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is null
            ? false
            : false;

    protected internal override PipelineResult Execute(PipelineResult pipelineResult)
    {
        pipelineResult.ConsoleHack.WriteLine("Not yet implemented");
        pipelineResult.SetSuccess();
        return pipelineResult;
    }
}
