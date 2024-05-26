// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public enum SubsystemKind
{
    Other = 0,
    Diagram,
    Completion,
    Help,
    Version,
    Validation,
    Invocation,
    ErrorReporting,
    Value,
    Response,
}

