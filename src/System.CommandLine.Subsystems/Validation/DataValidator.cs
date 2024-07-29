// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Validation;

public abstract class DataValidator
{
    public string Name { get; }
    public Type DataTraitType { get; }

    protected DataValidator(string name, Type dataTraitType)
    {
        Name = name;
        DataTraitType = dataTraitType;
    }

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

    public abstract IEnumerable<ParseError>? Validate(object? value, ValueResult? valueResult, DataTrait trait, ValidationContext validationContext);

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
    protected static List<ParseError> AddValidationError(ref List<ParseError>? parseErrors, string message, IEnumerable<object?> errorValues)
    {
        // TODO: Review the handling of errors. They are currently transient and returned by the Validate method, and to avoid allocating in the case of no errors (the common case) this method is used. This adds complexity to creating a new validator.
        parseErrors ??= new List<ParseError>();
        parseErrors.Add(new ParseError(message));
        return parseErrors;
    }
}

public abstract class DataValidator<TDataTrait>(string name) : DataValidator(name, typeof(TDataTrait))
    where TDataTrait : DataTrait
{
    protected TDataTrait GetTypedTraitOrThrow(DataTrait trait)
        => GetTypedTraitOrThrow<TDataTrait>(trait);

}
