// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace System.CommandLine.Subsystems;

public class PipelineResult(ParseResult? parseResult, string rawInput, Pipeline? pipeline, ConsoleHack? consoleHack = null)
{
    public ParseResult? ParseResult { get; } = parseResult;
    public string RawInput { get; } = rawInput;
    public Pipeline Pipeline { get; } = pipeline ?? Pipeline.CreateEmpty();
    public ConsoleHack ConsoleHack { get; } = consoleHack ?? new ConsoleHack();

    public bool AlreadyHandled { get; set; }
    public int ExitCode { get; set; }

    public void NotRun(ParseResult? parseResult)
    {
        // no op because defaults are false and 0
    }

    public void SetSuccess()
    {
        AlreadyHandled = true;
        ExitCode = 0;
    }
}
