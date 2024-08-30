// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation;

// TODO: Remove this class. All of the things it contains are in the PipelineResult, except the ValidationSubsystem currently
//       running, if there are multiple. The scenario where that is needed seems unlikely.
public class ValidationContext
{
    public ValidationContext(PipelineResult pipelineResult, ValidationSubsystem validationSubsystem)
    {
        PipelineResult = pipelineResult;
        ValidationSubsystem = validationSubsystem;
    }

    public PipelineResult PipelineResult { get; }
    public Pipeline Pipeline => PipelineResult.Pipeline;
    public ValidationSubsystem ValidationSubsystem { get; }
    public ParseResult? ParseResult => PipelineResult.ParseResult;
}
