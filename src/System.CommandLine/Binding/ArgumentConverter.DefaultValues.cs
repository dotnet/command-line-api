// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace System.CommandLine.Binding;

internal static partial class ArgumentConverter
{
    private static Array CreateEmptyArray(Type itemType)
        => Array.CreateInstance(itemType, 0);

    private static object CreateEmptyList(Type type)
    {
        var ctor = type
                   .GetConstructors()
                   .SingleOrDefault(c => c.GetParameters().Length == 0);

        if (ctor is { })
        {
            return ctor.Invoke(new object[] { });
        }

        throw new NotSupportedException($"You must register a custom binder for type {type}");
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
                                  Justification = $"{nameof(CreateDefaultValueType)} is only called on a ValueType. You can always create an instance of a ValueType.")]
    private static object? CreateDefaultValueType(Type type)
    {
        if (type.IsNullable())
        {
            return null;
        }

        return FormatterServices.GetUninitializedObject(type);
    }
}