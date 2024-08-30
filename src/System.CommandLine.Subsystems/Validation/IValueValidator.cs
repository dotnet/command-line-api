// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

/// <summary>
/// Interface that allows non-Validator derived methods to perform validation. Specifically, this supports
/// <see cref="CommandCondition"/> instances that can validate.
/// </summary>
public interface IValueValidator
{
    // Note: We pass both valueSymbol and valueResult, because we may validate symbols where valueResult is null.
    /// <summary>
    /// Validation method specific to value results.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueSymbol">The <see cref="CliValueSymbol"/> of the value to validate.</param>
    /// <param name="valueResult">The <see cref="CliValueResult"/> of the value to validate.</param>
    /// <param name="valueCondition">The <see cref="ValueCondition" that defines the condition that may be validatable.</param>
    /// <param name="validationContext">The <see cref="ValidationContext" containing information about the current context./></param>
    void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext);
}
