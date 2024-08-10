// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine;

public class PipelineResult(ParseResult? parseResult, string rawInput, Pipeline? pipeline, ConsoleHack? consoleHack = null)
{
    private readonly List<ParseError> errors = [];
    public ParseResult? ParseResult { get; } = parseResult;
    public string RawInput { get; } = rawInput;
    public Pipeline Pipeline { get; } = pipeline ?? Pipeline.CreateEmpty();
    public ConsoleHack ConsoleHack { get; } = consoleHack ?? new ConsoleHack();

    public bool AlreadyHandled { get; set; }
    public int ExitCode { get; set; }

    public void AddErrors(IEnumerable<ParseError> errors)
    {
        if (errors is not null)
        {
            this.errors.AddRange(errors);
        }
    }

    public void AddError(ParseError error)
        => errors.Add(error);

    public IEnumerable<ParseError> GetErrors(bool excludeParseErrors = false)
        => excludeParseErrors || ParseResult is null
            ? errors
            : ParseResult.Errors.Concat(errors);

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
