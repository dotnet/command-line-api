// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Parsing;

internal static class ValueResultExtensions
{
    internal static ValueResultOutcome GetValueResultOutcome(ArgumentConversionResultType? resultType)
        => resultType switch
        {
            ArgumentConversionResultType.NoArgument => ValueResultOutcome.NoArgument,
            ArgumentConversionResultType.Successful => ValueResultOutcome.Success,
            _ => ValueResultOutcome.HasErrors
        };
}
