// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.CommandLine.Binding;

internal static partial class ArgumentConverter
{
#if NET6_0_OR_GREATER
    private static readonly Lazy<ConstructorInfo> _listCtor =
        new(() => typeof(List<>)
                  .GetConstructors()
                  .SingleOrDefault(c => c.GetParameters().Length == 0)!);
#endif

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "https://github.com/dotnet/command-line-api/issues/1638")]
    private static Array CreateArray(Type itemType, int capacity)
        => Array.CreateInstance(itemType, capacity);

    private static IList CreateEmptyList(Type listType)
    {
#if NET6_0_OR_GREATER
        var ctor = (ConstructorInfo)listType.GetMemberWithSameMetadataDefinitionAs(_listCtor.Value);
#else
        var ctor = listType
                   .GetConstructors()
                   .SingleOrDefault(c => c.GetParameters().Length == 0);
#endif

        return (IList)ctor.Invoke(new object[] { });
    }

    private static IList CreateEnumerable(Type type, Type itemType, int capacity = 0)
    {
        if (type.IsArray)
        {
            return CreateArray(itemType, capacity);
        }

        if (type.IsGenericType)
        {
            var x = type.GetGenericTypeDefinition() switch
            {
                { } enumerable when typeof(IEnumerable<>).IsAssignableFrom(enumerable) =>
                    CreateArray(itemType, capacity),
                { } array when typeof(IList<>).IsAssignableFrom(array) ||
                               typeof(ICollection<>).IsAssignableFrom(array) =>
                    CreateArray(itemType, capacity),
                { } list when list == typeof(List<>) =>
                    CreateEmptyList(type),
                _ => null
            };

            if (x is { })
            {
                return x;
            }
        }

        throw new ArgumentException($"Type {type} cannot be created without a custom binder.");
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
                                  Justification = $"{nameof(CreateDefaultValueType)} is only called on a ValueType. You can always create an instance of a ValueType.")]
    private static object CreateDefaultValueType(Type type) =>
        FormatterServices.GetUninitializedObject(type);
}