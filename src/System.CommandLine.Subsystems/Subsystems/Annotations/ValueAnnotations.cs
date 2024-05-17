// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class ValueAnnotations
{
    internal static string Prefix { get; } = nameof(SubsystemKind.Value);

    /// <summary>
    /// Default value for an option or argument
    /// </summary>
    /// <remarks>
    /// Although the type is <see cref="object?"/>, it must actually be the same type as the type
    /// parameter of the <see cref="CliArgument{T}"/> or <see cref="CliOption{T}"/>.
    /// </remarks>
    public static AnnotationId<object?> DefaultValue { get; } = new(Prefix, nameof(DefaultValue));

    /// <summary>
    /// Default value calculation for an option or argument
    /// </summary>
    /// <remarks>
    /// Although the type is <see cref="object?"/>, it must actually be a <see cref="Func{TResult}">
    /// with a type parameter matching the the type parameter type of the <see cref="CliArgument{T}"/>
    /// or <see cref="CliOption{T}"/>
    /// </remarks>
    public static AnnotationId<object> DefaultValueCalculation { get; } = new(Prefix, nameof(DefaultValueCalculation));
}
