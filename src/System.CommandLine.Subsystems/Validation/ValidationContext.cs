// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation
{
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
}
