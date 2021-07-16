// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Parsing
{
    internal static class OptionResultExtensions
    {
        internal static ArgumentConversionResult ConvertIfNeeded(
            this OptionResult optionResult,
            Type type) =>
            optionResult.ArgumentConversionResult
                .ConvertIfNeeded(optionResult, type);
    }
}