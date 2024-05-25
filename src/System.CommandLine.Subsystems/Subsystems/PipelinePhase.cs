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



// AddSubsystem(CliSubsystem subsystem, SubsystemPhase phase = SubsystemPhase.NotSpecified);

//public enum SubsystemPhase
//{
//    NotSpecified = 0,
//    BeforeDiagram,
//    Diagram,
//    AfterDiagram,
//    BeforeCompletion,
//    Completion,
//    AfterCompletion,
//    BeforeHelp,
//    Help,
//    AfterHelp,
//    BeforeVersion,
//    Version,
//    AfterVersion,
//    BeforeErrorReporting,
//    ErrorReporting,
//    AfterErrorReporting,
//}

// AddSubsystem(CliSubsystem subsystem, SubsystemPhase phase = SubsystemPhase.NotSpecified, PhaseTiming timing = PhaseTiming.Before);

//public enum SubsystemPhase
//{
//    NotSpecified = 0,
//    Diagram,
//    Completion,
//    Help,
//    Version,
//    ErrorReporting,
//}

//public enum PhaseTiming
//{
//    Before = 0,
//    After
//}


///// <summary>
///// Subsystem phases group subsystems that should be run at specific places in CLI processing and 
///// are used for high level ordering.
///// </summary>
///// <remarks>
///// Order of operations:
///// 
///// * Initialize is called for all subsystems, regardless of phase
///// * ExecuteIfNeeded is called for subsystems in the EarlyReturn phase
///// * ExecuteIfNeeded is called for subsystems in the Validate phase
///// * ExecuteIfNeeded is called for subsystems in the Execute phase
///// * ExecuteIfNeeded is called for subsystems in the Finish phase
///// * Teardown is called for all subsystems, regardless of phase
///// </remarks>
//public enum SubsystemPhase
//{
//    /// <summary>
//    /// Indicates a subsystem that never runs, and exists to support other subsystems. ValueSubsystem
//    /// is an example.
//    /// </summary>
//    /// <remarks>
//    /// Initialization runs first, teardown runs last - this is arbitrary and can be changed prior
//    /// to release if we have scenarios to justify.
//    /// </remarks>
//    None,

//    /// <summary>
//    /// Indicates a subsystem is designed to shortcut execution and perform an action other than the 
//    /// action indicated by the command. HelpSubsystem and VersionSubsystem are examples.
//    /// </summary>
//    /// <remarks>
//    /// EarlyReturn subsystems are differentiated from other execution because data validation has not
//    /// occurred. Because of this, data should not be used and should be assume to be questionable.
//    /// </remarks>
//    EarlyReturn,

//    /// <summary>
//    /// Indicates a subsystem that validates data entered by the user. 
//    /// </summary>
//    /// <remarks>
//    /// Errors are not reported, but are rather stored for later display. This may be reconsidered
//    /// if we keep track of which errors have been reported.
//    /// </remarks>
//    Validate,

//    /// <summary>
//    /// Indicates a subsystem that executes using data entered by the user. The only known case is 
//    /// the Invocation subsystem.
//    /// </summary>
//    Execute,

//    /// <summary>
//    /// Indicates a subsystem that runs as the CLI part of processing is ending. ErrorReportingSubsystem
//    /// is an example, although we may rethink when errors are displayed.
//    /// </summary>
//    /// <remarks>
//    /// This is separate from the TearDown step, which is avaiable to all subsystems.
//    /// </remarks>
//    Finish,
//}


