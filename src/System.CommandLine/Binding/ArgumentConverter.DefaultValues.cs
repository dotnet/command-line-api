// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.CommandLine.Binding;

internal static partial class ArgumentConverter
{
#if NET6_0_OR_GREATER
    private static ConstructorInfo? _listCtor;
#endif

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "https://github.com/dotnet/command-line-api/issues/1638")]
    private static Array CreateArray(Type itemType, int capacity)
        => Array.CreateInstance(itemType, capacity);

    private static IList CreateEmptyList(Type listType)
    {
#if NET6_0_OR_GREATER
        ConstructorInfo? listCtor = _listCtor;

        if (listCtor is null)
        {
            _listCtor = listCtor = typeof(List<>).GetConstructor(Type.EmptyTypes)!;
        }

        var ctor = (ConstructorInfo)listType.GetMemberWithSameMetadataDefinitionAs(listCtor);
#else
        var ctor = listType.GetConstructor(Type.EmptyTypes);
#endif

        return (IList)ctor.Invoke(null);
    }

    internal static IList CreateEnumerable(Type type, Type itemType, int capacity = 0)
    {
        if (type.IsArray)
        {
            return CreateArray(itemType, capacity);
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(IEnumerable<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(ICollection<>))
            {
                return CreateArray(itemType, capacity);
            }

            if (genericTypeDefinition == typeof(List<>))
            {
                return CreateEmptyList(type);
            }
        }

        throw new ArgumentException($"Type {type} cannot be created without a custom binder.");
    }
}