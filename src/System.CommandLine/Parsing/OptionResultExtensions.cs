// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing
{
    public static class OptionResultExtensions
    {
        internal static ArgumentConversionResult ConvertIfNeeded(
            this OptionResult optionResult,
            Type type)
        {
            if (optionResult == null)
            {
                throw new ArgumentNullException(nameof(optionResult));
            }

            return optionResult.ArgumentConversionResult
                               .ConvertIfNeeded(optionResult, type);
        }

        public static object? GetValueOrDefault(this OptionResult optionResult)
        {
            return optionResult.GetValueOrDefault<object?>();
        }

        [return:MaybeNull]
        public static T GetValueOrDefault<T>(this OptionResult optionResult)
        {
            return optionResult.ConvertIfNeeded(typeof(T))
                               .GetValueOrDefault<T>();
        }
    }
}
