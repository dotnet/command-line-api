// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems.DataTraits;

namespace System.CommandLine.Subsystems.Validation; 

public abstract class Validator
{
    protected object? GetValue(CliDataSymbol symbol, Pipeline pipeline) 
        => pipeline.Value.GetValue(symbol);

    public abstract IEnumerable<ParseError>? Validate<T>(CliSymbol symbol, DataTrait trait, Pipeline pipeline, ValidationSubsystem validationSubsystem);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parseErrors"></param>
    /// <param name="message"></param>
    /// <param name="errorValues"></param>
    /// <returns></returns>
    /// <remarks>
    /// This method needs to be evolved as we replace ParseError with CliError
    /// </remarks>
    public static List<ParseError> AddValidationError(List<ParseError>? parseErrors, string message, IEnumerable<object?> errorValues)
    {
        parseErrors ??= new List<ParseError>();
        parseErrors.Add(new ParseError(message));
        return parseErrors;
    }
}
