// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

public abstract class ValueValidator : Validator
{
    protected ValueValidator(string name, Type valueConditionType, params Type[] moreValueConditionTypes)
        : base(name, valueConditionType, moreValueConditionTypes)
    { }

    // These methods provide consistent messages
    protected TDataValueCondition GetTypedValueConditionOrThrow<TDataValueCondition>(ValueCondition valueCondition)
        where TDataValueCondition : ValueCondition
        => valueCondition is TDataValueCondition typedValueCondition
            ? typedValueCondition
            : throw new ArgumentException($"{Name} validation failed to find bounds");

    protected TValue GetValueAsTypeOrThrow<TValue>(object? value)
        => value is TValue typedValue
            ? typedValue
            : throw new InvalidOperationException($"{Name} validation does not apply to this type");

    public abstract void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext);
}
