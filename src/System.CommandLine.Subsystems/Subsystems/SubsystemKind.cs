// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public enum SubsystemKind
{
    Other = 0,
    Help,
    Version,
    Value,
    ErrorReporting,
    Completion,
    Diagram,
    Response
}
