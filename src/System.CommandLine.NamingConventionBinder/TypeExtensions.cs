// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides extensions to <see cref="Type"/>.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Determines whether <paramref name="type"/> is a constructed type of <paramref name="genericTypeDefinition"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="genericTypeDefinition">The generic type definition.</param>
    /// <returns><see langword="true" /> if <paramref name="type"/> is a constructed type of <paramref name="genericTypeDefinition"/>; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsConstructedGenericTypeOf(this Type type, Type genericTypeDefinition)
        => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;

    /// <summary>
    /// Determines whether <paramref name="type"/> is a nullable value type.
    /// </summary>
    /// <param name="type">The self.</param>
    /// <returns><see langword="true" /> if <paramref name="type"/> is a nullable value type; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullableValueType(this Type type)
        => type.IsValueType && type.IsConstructedGenericTypeOf(typeof(Nullable<>));
}