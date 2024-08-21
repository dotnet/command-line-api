// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

public interface IValueValidator
{
    void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext);
}
