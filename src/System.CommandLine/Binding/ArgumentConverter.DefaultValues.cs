// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Binding;

internal static partial class ArgumentConverter
{
    private static Array CreateEmptyArray(Type itemType)
        => Array.CreateInstance(itemType, 0);

    private static object CreateEmptyList(Type type)
    {
        if (type == typeof(List<string>))
        {
            return new List<string>();
        }

        if (type == typeof(List<int>))
        {
            return new List<string>();
        }

        throw new NotSupportedException($"You must register a custom binder for type {type}");
    }

    private static object? CreateDefaultValueType(Type type)
    {
        if (type.IsNullable())
        {
            return null;
        }

        if (type == typeof(bool)) return false;
        if (type == typeof(int)) return 0;
        if (type == typeof(double)) return 0d;
        if (type == typeof(ulong)) return 0ul;
        if (type == typeof(byte)) return (byte)0;
        if (type == typeof(decimal)) return 0m;
        if (type == typeof(float)) return 0f;

        throw new NotSupportedException($"You must register a custom binder for type {type}");
    }
}