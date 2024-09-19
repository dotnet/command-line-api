// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Additional context that is passed to <see cref="IAnnotationProvider"/>.
/// </summary>
/// <remarks>
/// This class exists so that additional context properties can be added without
/// breaking existing <see cref="IAnnotationProvider"/> implementations.
/// <para>
/// This is intended to be usable independently of the pipeline. For example, a method could be
/// implemented that takes a <see cref="CommandLine.ParseResult"/> and prints help output based on the help
/// annotations in the <see cref="CliSymbol"/> tree, which would then be usable by developers who
/// are using the <see cref="CliParser"/> API directly.
/// </para>
/// </remarks>
public class AnnotationResolveContext(ParseResult parseResult)
{
    public AnnotationResolveContext(PipelineResult pipelineResult)
    : this(pipelineResult.ParseResult)
    {
    }

    /// <summary>
    /// The <see cref="ParseResult"/> for which annotations are being resolved.
    /// </summary>
    /// <remarks>
    /// This may be used to resolve different values for an annotation based on the parents of the symbol,
    /// or based on values of other symbols in the parse result.
    /// </remarks>
    public ParseResult ParseResult { get; } = parseResult;
}
