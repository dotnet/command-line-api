// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

internal class PipelinePhase(SubsystemKind kind)
{
    private List<CliSubsystem>? before = null;
    private List<CliSubsystem>? after = null;

    public readonly SubsystemKind Kind = kind;
    protected CliSubsystem? CliSubsystem { get; set; }

    /// <summary>
    /// Add a subsystem to the phase
    /// </summary>
    /// <param name="subsystem">The subsystem to add</param>
    /// <param name="timing">Whether it should run before or after the key subsystem</param>
    /// <remarks>
    /// Adding a subsystem that is not of the normal phase type is expected and OK.
    /// </remarks>
    internal void AddSubsystem(CliSubsystem subsystem, AddToPhaseBehavior timing)
    {
        timing = timing == AddToPhaseBehavior.SubsystemRecommendation ? subsystem.RecommendedAddToPhaseBehavior : timing;
        List<CliSubsystem>? addToList = timing == AddToPhaseBehavior.Prepend
            ? CreateBeforeIfNeeded()
            : CreateAfterIfNeeded();

        addToList.Add(subsystem);
    }

    private List<CliSubsystem> CreateBeforeIfNeeded()
    {
        before ??= [];
        return before;
    }

    private List<CliSubsystem> CreateAfterIfNeeded()
    {
        after ??= [];
        return after;
    }

    public IEnumerable<CliSubsystem> GetSubsystems()
        => [
            .. (before is null ? [] : before),
            .. (CliSubsystem is null ? new List<CliSubsystem> { } : [CliSubsystem]),
            .. (after is null ? [] : after)
            ];
}

/// <summary>
/// This manages one phase. The most common case is that it is empty, and the most complicated
/// case of several items before, and several items after will be quite rare.
/// </summary>
/// <remarks>
/// <para>
/// The most common case is that it is empty, and the most complicated
/// case of several items before, and several items after will be quite rare. <br/>
/// </para>
/// <para>
/// In the current design, this needs to be a reference type so values are synced.
/// </para>
/// </remarks>
internal class PipelinePhase<TSubsystem> : PipelinePhase
    where TSubsystem : CliSubsystem
{
    private TSubsystem? subsystem;

    public PipelinePhase(SubsystemKind kind) : base(kind)
    { }

    internal TSubsystem? Subsystem
    {
        get => subsystem;
        set
        {
            CliSubsystem = value;
            subsystem = value;
        }
    }
}
