// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Validation;

public abstract class CommandValidator
{
    public string Name { get; }

    protected CommandValidator(string name)
        => Name = name;

    // These methods provide consistent messages
    protected CliDataSymbol GetDataSymbolOrThrow(CliSymbol symbol)
        => symbol is CliDataSymbol dataSymbol
            ? dataSymbol
            : throw new ArgumentException($"{Name} validation only works on options and arguments");

    protected TDataTrait GetTypedTraitOrThrow<TDataTrait>(DataTrait trait)
        where TDataTrait : DataTrait
        => trait is TDataTrait typedTrait
            ? typedTrait
            : throw new ArgumentException($"{Name} validation failed to find bounds");

    protected TValue GetValueAsTypeOrThrow<TValue>(object? value)
        => value is TValue typedValue
            ? typedValue
            : throw new InvalidOperationException($"{Name} validation does not apply to this type");

    public abstract IEnumerable<ParseError>? Validate(CommandValueResult commandResult, DataTrait trait, ValidationContext validationContext);

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
