﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

/// <summary>
/// Subsystem for reporting errors
/// </summary>
/// <remarks>
/// This class, including interface, is likey to change as powderhouse continues
/// </remarks>
public class ErrorReportingSubsystem : CliSubsystem
{
    public ErrorReportingSubsystem()
        : base(ErrorReportingAnnotations.Prefix, SubsystemKind.ErrorReporting)
    { }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.Errors.Any();

    // TODO: properly test execute directly when parse result is usable in tests
    public override void Execute(PipelineResult pipelineResult)
    {
        var _ = pipelineResult.ParseResult
            ?? throw new ArgumentException("The parse result has not been set", nameof(pipelineResult));

        Report(pipelineResult.ConsoleHack, pipelineResult.ParseResult.Errors);

        pipelineResult.SetSuccess();
    }

    public void Report(ConsoleHack consoleHack, IReadOnlyList<CliDiagnostic> errors)
    {
        ConsoleHelpers.ResetTerminalForegroundColor();
        ConsoleHelpers.SetTerminalForegroundRed();

        foreach (var error in errors)
        {
            consoleHack.WriteLine(error.Message);
        }
        consoleHack.WriteLine();

        ConsoleHelpers.ResetTerminalForegroundColor();
    }
}
