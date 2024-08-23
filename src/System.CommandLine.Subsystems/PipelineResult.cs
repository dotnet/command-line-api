// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine;

public class PipelineResult
{
    // TODO: Try to build workflow so it is illegal to create this without a ParseResult
    private readonly List<ParseError> errors = [];
    private ValueProvider valueProvider { get; } 

    public PipelineResult(ParseResult parseResult, string rawInput, Pipeline? pipeline, ConsoleHack? consoleHack = null)
    {
        ParseResult = parseResult;
        RawInput = rawInput;
        Pipeline = pipeline ?? Pipeline.CreateEmpty();
        ConsoleHack = consoleHack ?? new ConsoleHack();
        valueProvider = new ValueProvider(this);
    }

    public ParseResult ParseResult { get; }
    public string RawInput { get; }

    // TODO: Consider behavior when pipeline is null - this is probably a core user accessing some subsystems
    public Pipeline Pipeline { get; }
    public ConsoleHack ConsoleHack { get; }

    public bool AlreadyHandled { get; set; }
    public int ExitCode { get; set; }

    public T? GetValue<T>(CliValueSymbol dataSymbol)
     => valueProvider.GetValue<T>(dataSymbol);

    public object? GetValue(CliValueSymbol option)
        => valueProvider.GetValue<object>(option);

    public CliValueResult? GetValueResult(CliValueSymbol dataSymbol)
     => ParseResult.GetValueResult(dataSymbol);


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
