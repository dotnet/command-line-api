// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing
{
    public static class ArgumentResultExtensions
    {
        [return: MaybeNull]
        public static object? GetValueOrDefault(this ArgumentResult argumentResult) =>
            argumentResult.GetValueOrDefault<object?>();

        [return: MaybeNull]
        public static T GetValueOrDefault<T>(this ArgumentResult argumentResult) =>
            argumentResult.GetArgumentConversionResult()
                          .ConvertIfNeeded(argumentResult, typeof(T))
                          .GetValueOrDefault<T>();
    }
}
