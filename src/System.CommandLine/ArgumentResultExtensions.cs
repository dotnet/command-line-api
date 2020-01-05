// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentResultExtensions
    {
        public static object GetValueOrDefault(this ArgumentResult argumentResult) => 
            argumentResult.GetValueOrDefault<object>();

        public static T GetValueOrDefault<T>(this ArgumentResult argumentResult) =>
            argumentResult
                .ArgumentConversionResult
                .ConvertIfNeeded(argumentResult, typeof(T))
                .GetValueOrDefault<T>();
    }
}
