// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation;
using System.CommandLine.ValueSources;
using System.Globalization;

namespace System.CommandLine.ValueConditions;

///// <summary>
///// Declares the string casing condition for the option or argument.
///// </summary>
///// <remarks>
///// The non-generic version is used by the <see cref="StringValidator/>.
///// </remarks>
//public abstract class StringCase : ValueCondition
//{
//    protected StringCase(string valueType)
//        : base(nameof(StringCase))
//    {
//        ValueType = valueType;
//    }

//    /// <summary>
//    /// The type of the symbol the casing applies to.
//    /// </summary>
//    public string ValueType { get; }
//}

/// <summary>
/// Declares the casing condition for the option or argument. Instances 
/// of this method are created via extension methods on <see cref="ValueSymbol"/>
/// </summary>
/// <typeparam name="T">The type of the symbol the range applies to.</typeparam>
public class StringCase : ValueCondition, IValueValidator
{
    internal StringCase(ValueSource<string>? casing) : base(nameof(StringCase))
    {
        Casing = casing;
    }

    /// <inheritdoc/>
    public void Validate(object? value,
                         CliValueSymbol valueSymbol,
                         CliValueResult? valueResult,
                         ValueCondition valueCondition,
                         ValidationContext validationContext)
    {
        if (valueCondition != this) throw new InvalidOperationException("Unexpected value condition type");
        if (value is not string stringValue) throw new InvalidOperationException("Unexpected value type");

        if (stringValue is null) return; // nothing to do

        // TODO: Replace the strings we are comparing with a diagnostic ID when we update ParseError
        if (Casing is not null
            && validationContext.TryGetTypedValue(Casing, out var casingValue))
        {
            if (casingValue is null) return;
            if (casingValue == "lower"
                && !stringValue.Equals(stringValue.ToLower(CultureInfo.CurrentCulture)))
            {
                validationContext.AddError(new ParseError($"The value for '{valueSymbol.Name}' is not in lower case."));
            }
            if (casingValue == "upper"
                && !stringValue.Equals(stringValue.ToUpper(CultureInfo.CurrentCulture)))
            {
                validationContext.AddError(new ParseError($"The value for '{valueSymbol.Name}' is not in upper case."));
            }
        }
    }

    /// <summary>
    /// The expected casing.
    /// </summary>
    public ValueSource<string>? Casing { get; init; }
}
