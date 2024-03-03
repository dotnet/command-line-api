// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;

namespace System.CommandLine;

    // TODO: Consider what info is needed after invocation. If it's the whole pipeline context, consider collapsing this with that class.
public class CliExit
{
    internal CliExit(PipelineContext pipelineContext)
        : this(pipelineContext.ParseResult, pipelineContext.AlreadyHandled, pipelineContext.ExitCode)
    { }

    private CliExit(ParseResult? parseResult, bool handled, int exitCode)
    {
        ExitCode = exitCode;
        Handled = handled;
        ParseResult = parseResult;
    }
    public ParseResult? ParseResult { get; set; }

    public int ExitCode { get; }

    public static implicit operator int(CliExit cliExit) => cliExit.ExitCode;

    public static implicit operator bool(CliExit cliExit) => !cliExit.Handled;


    public bool Handled { get; }

    public static CliExit NotRun(ParseResult? parseResult) => new(parseResult, false, 0);

    public static CliExit SuccessfullyHandled(ParseResult? parseResult) => new(parseResult, true, 0);
}
