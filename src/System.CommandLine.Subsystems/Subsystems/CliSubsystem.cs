// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems;

/// <summary>
/// Base class for CLI subsystems. Implements storage of annotations.
/// </summary>
/// <param name="annotationProvider"></param>
public abstract class CliSubsystem
{
    protected CliSubsystem(string name, SubsystemKind subsystemKind, IAnnotationProvider? annotationProvider)
    {
        Name = name;
        _annotationProvider = annotationProvider;
        Kind = subsystemKind;
    }

    /// <summary>
    /// The name of the subsystem.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Defines the kind of subsystem, such as help or version
    /// </summary>
    public SubsystemKind Kind { get; }
    public AddToPhaseBehavior RecommendedAddToPhaseBehavior { get; }

    private readonly IAnnotationProvider? _annotationProvider;

    /// <summary>
    /// Attempt to retrieve the <paramref name="symbol"/>'s value for the annotation <paramref name="id"/>. This will check the
    /// annotation provider that was passed to the subsystem constructor, and the internal annotation storage.
    /// </summary>
    /// <typeparam name="TValue">The value of the type to retrieve</typeparam>
    /// <param name="symbol">The symbol the value is attached to</param>
    /// <param name="id">
    /// The identifier for the annotation value to be retrieved.
    /// For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">An out parameter to contain the result</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Subsystem authors must use this to access annotation values, as it respects the subsystem's <see cref="IAnnotationProvider"/> if it has one.
    /// This value is protected because it is intended for use only by subsystem authors. It calls <see cref="AnnotationStorageExtensions"/>
    /// </remarks>
    protected internal bool TryGetAnnotation<TValue>(CliSymbol symbol, AnnotationId<TValue> annotationId, [NotNullWhen(true)] out TValue? value)
    {
        if (_annotationProvider is not null && _annotationProvider.TryGet(symbol, annotationId, out value))
        {
            return true;
        }

        return symbol.TryGetAnnotation(annotationId, out value);
    }

    /// <summary>
    /// The subystem executes, even if another subsystem has handled the operation. This is expected to be used in things like error reporting.
    /// </summary>
    protected internal virtual bool RunsEvenIfAlreadyHandled { get; protected set; }

    /// <summary>
    /// Executes the behavior of the subsystem. For example, help would write information to the console.
    /// </summary>
    /// <param name="pipelineResult">The context contains data like the ParseResult, and allows setting of values like whether execution was handled and the CLI should terminate </param>
    /// <returns>A PipelineResult object with information such as whether the CLI should terminate</returns>
    protected internal virtual void Execute(PipelineResult pipelineResult)
        => pipelineResult.NotRun(pipelineResult.ParseResult);

    internal PipelineResult ExecuteIfNeeded(PipelineResult pipelineResult)
        => ExecuteIfNeeded(pipelineResult.ParseResult, pipelineResult);

    internal PipelineResult ExecuteIfNeeded(ParseResult? parseResult, PipelineResult pipelineResult)
    {
        if (GetIsActivated(parseResult))
        {
            Execute(pipelineResult);
        }
        return pipelineResult;
    }


    /// <summary>
    /// Indicates to invocation patterns that the extension should be run.
    /// </summary>
    /// <remarks>
    /// This may be explicitly set, such as a directive like Diagram, or it may explore the result
    /// </remarks>
    /// <param name="result">The parse result.</param>
    /// <returns></returns>
    protected internal virtual bool GetIsActivated(ParseResult? parseResult) => false;

    /// <summary>
    /// Runs before parsing to prepare the parser. Since it always runs, slow code that is only needed when the extension 
    /// runs as part of invocation should be delayed to BeforeRun(). Default behavior is to do nothing.
    /// </summary>
    /// <remarks>
    /// Use cases:
    /// * Add to the CLI, such as adding version option
    /// * Early setup of extension internal data, such as reading a file that contains defaults
    /// * Licensing if early exit is needed
    /// </remarks>
    /// <param name="configuration">The CLI configuration, which contains the RootCommand for customization</param>
    /// <returns>True if parsing should continue</returns> // there might be a better design that supports a message
    // TODO: Because of this and similar usage, consider combining CLI declaration and config. ArgParse calls this the parser, which I like
    // TODO: Why does Intitialize return a configuration?
    protected internal virtual void Initialize(InitializationContext context)
    { }

    // TODO: Determine if this is needed.
    protected internal virtual void TearDown(PipelineResult pipelineResult)
    { }

}
