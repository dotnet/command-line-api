// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation
{
    public class ValidationContext
    {
        public ValidationContext(Pipeline pipeline, ValidationSubsystem validationSubsystem, ParseResult? parseResult)
        {
            Pipeline = pipeline;
            ValidationSubsystem = validationSubsystem;
            ParseResult = parseResult;
        }

        public Pipeline Pipeline { get; }
        public ValidationSubsystem ValidationSubsystem { get; }
        public ParseResult? ParseResult { get; }
    }
}
