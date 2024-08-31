// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

/// <summary>
/// Base class for validators that affect a single symbol.
/// </summary>
public abstract class ValueValidator : Validator
{
    protected ValueValidator(string name, Type valueConditionType, params Type[] moreValueConditionTypes)
        : base(name, valueConditionType, moreValueConditionTypes)
    { }

    protected TValue GetValueAsTypeOrThrow<TValue>(object? value)
        => value is TValue typedValue
            ? typedValue
            : throw new InvalidOperationException($"{Name} validation does not apply to this type");

    /// <summary>
    /// Validation method specific to a single symbols value.results
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueSymbol">The option or argument being validated.</param>
    /// <param name="valueResult">The <see cref="CliValueResult" that will be validated./></param>
    /// <param name="valueCondition">The <see cref="ValueCondition" that defines the condition that may be validatable./></param>
    /// <param name="validationContext">The <see cref="ValidationContext" containing information about the current context./></param>
    public abstract void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext);
}

