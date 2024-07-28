// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Validation;

public abstract class Validator
{
    public string Name { get; }

    protected Validator(string name)
        => Name = name;

    // These methods provide consistent messages
    protected CliDataSymbol GetDataSymbolOrThrow(CliSymbol symbol)
        => symbol is CliDataSymbol dataSymbol
            ? dataSymbol
            : throw new ArgumentException($"{Name} validation only works on options and arguments");

    protected TDataTrait GetDataTraitOrThrow<TDataTrait>(DataTrait trait)
        where TDataTrait : DataTrait
        => trait is TDataTrait typedTrait
            ? typedTrait
            : throw new ArgumentException($"{Name} validation failed to find bounds");

    protected TValue GetValueAsTypeOrThrow<TValue>(CliDataSymbol dataSymbol, Pipeline pipeline)
        => pipeline.Value.GetValue(dataSymbol) is TValue typedValue
            ? typedValue
            : throw new InvalidOperationException($"{Name} validation only works on options and arguments that can be compared");



    public abstract IEnumerable<ParseError>? Validate(CliSymbol symbol, DataTrait trait, Pipeline pipeline, ValidationSubsystem validationSubsystem);

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
