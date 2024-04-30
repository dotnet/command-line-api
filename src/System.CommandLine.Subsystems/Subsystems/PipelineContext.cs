// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public class PipelineContext(ParseResult? parseResult, string rawInput, Pipeline? pipeline, ConsoleHack? consoleHack = null)
{
    public ParseResult? ParseResult { get; } = parseResult;
    public string RawInput { get; } = rawInput;
    public Pipeline Pipeline { get; } = pipeline ?? Pipeline.CreateEmpty();
    public ConsoleHack ConsoleHack { get; } = consoleHack ?? new ConsoleHack();

    public bool AlreadyHandled { get; set; }
    public int ExitCode { get; set; }

}
