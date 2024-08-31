// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Default Value annotations.
/// </summary>
public static class ValueAnnotations
{
    internal static string Prefix { get; } = nameof(SubsystemKind.Value);

    /// <summary>
    /// Default value source, which may be an aggregate source, for an option or argument
    /// </summary>
    public static AnnotationId DefaultValueSource { get; } = new(Prefix, nameof(DefaultValueSource));

    /// <summary>
    /// Default default value for an option or argument
    /// </summary>
    /// <remarks>
    /// Should be the same type as the type parameter of
    /// the <see cref="CliArgument{T}"/> or <see cref="CliOption{T}"/>.
    /// </remarks>
    public static AnnotationId DefaultValue { get; } = new(Prefix, nameof(DefaultValue));


    /// <summary>
    /// Default default value calculation for an option or argument
    /// </summary>
    /// <remarks>
    /// Please use the extension methods and do not call this directly.
    /// <para>
    /// Should use a <see cref="Func{TValue}"> with the same type parameter as
    /// the <see cref="CliArgument{T}"/> or <see cref="CliOption{T}"/>.
    /// </para>
    /// </remarks>
    public static AnnotationId DefaultValueCalculation { get; } = new(Prefix, nameof(DefaultValueCalculation));
}
