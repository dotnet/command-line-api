// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.Traits;

namespace System.CommandLine.Validation;

public abstract class CommandValidator
{
    public string Name { get; }
    public Type TraitType { get; }

    protected CommandValidator(string name, Type traitType)
    {
        Name = name;
        TraitType = traitType;
    }

    // These methods provide consistent messages
    protected CliCommand GetSymbolOrThrow(CliSymbol symbol)
        => symbol is CliCommand comandSymbol
            ? comandSymbol
            : throw new ArgumentException($"{Name} validation only works on commands.");

    protected TTrait GetTypedTraitOrThrow<TTrait>(Trait trait)
        where TTrait : Trait
        => trait is TTrait typedTrait
            ? typedTrait
            : throw new ArgumentException($"{Name} validation failed to find bounds");

    public abstract IEnumerable<ParseError>? Validate(CommandValueResult commandResult, Trait trait, ValidationContext validationContext);

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
    public static List<ParseError> AddValidationError(ref List<ParseError>? parseErrors, string message, IEnumerable<object?> errorValues)
    {
        parseErrors ??= new List<ParseError>();
        parseErrors.Add(new ParseError(message));
        return parseErrors;
    }
}

public abstract class CommandValidator<TCommandTrait>(string name) 
    : CommandValidator(name, typeof(TCommandTrait))
    where TCommandTrait : Trait
{
    protected TCommandTrait GetTypedTraitOrThrow(Trait trait)
        => GetTypedTraitOrThrow<TCommandTrait>(trait);
}
