// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
    public ErrorReportingSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(ErrorReportingAnnotations.Prefix, SubsystemKind.ErrorReporting, annotationProvider)
    { }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.Errors.Any();

    // TODO: properly test execute directly when parse result is usable in tests
    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        var _ = pipelineContext.ParseResult
            ?? throw new ArgumentNullException($"{nameof(pipelineContext)}.ParseResult");

        Report(pipelineContext.ConsoleHack, pipelineContext.ParseResult.Errors);

        return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
    }

    public void Report(ConsoleHack consoleHack, IReadOnlyList<ParseError> errors)
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
