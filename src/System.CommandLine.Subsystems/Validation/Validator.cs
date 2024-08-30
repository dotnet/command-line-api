using System.CommandLine.Parsing;
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Validation;

// TODO: This may be removed if we settle on ValueCondition validation only.
/// <summary>
/// Base class for CommandValidator and ValueValidator. 
/// </summary>
// TODO: Discuss visibility and custom validators
public abstract class Validator
{
    public Validator(string name, Type valueConditionType, params Type[] moreValueConditionTypes)
    {
        Name = name;
        ValueConditionTypes = [valueConditionType, .. moreValueConditionTypes];
    }

    public string Name { get; }

    public Type[] ValueConditionTypes { get; }

    /// <summary>
    /// Adds a validation CliDiagnostic that will alter be added to the PipelineResult. Not yet implemented to support that
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

    // These methods provide consistent messages
    protected TCommandCondition GetTypedValueConditionOrThrow<TCommandCondition>(CommandCondition commandCondition)
        where TCommandCondition : CommandCondition
        => commandCondition is TCommandCondition typedValueCondition
            ? typedValueCondition
            : throw new ArgumentException($"{Name} validation failed to validator");

    // These methods provide consistent messages
    protected TDataValueCondition GetTypedValueConditionOrThrow<TDataValueCondition>(ValueCondition valueCondition)
        where TDataValueCondition : ValueCondition
        => valueCondition is TDataValueCondition typedValueCondition
            ? typedValueCondition
            : throw new ArgumentException($"{Name} validation failed to find bounds");


}
