// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

public partial class Pipeline
{
    private readonly PipelinePhase<DiagramSubsystem> diagramPhase = new(SubsystemKind.Diagram);
    private readonly PipelinePhase<CompletionSubsystem> completionPhase = new(SubsystemKind.Completion);
    private readonly PipelinePhase<HelpSubsystem> helpPhase = new(SubsystemKind.Help);
    private readonly PipelinePhase<VersionSubsystem> versionPhase = new(SubsystemKind.Version);
    private readonly PipelinePhase<ValidationSubsystem> validationPhase = new(SubsystemKind.Validation);
    private readonly PipelinePhase<InvocationSubsystem> invocationPhase = new(SubsystemKind.Invocation);
    private readonly PipelinePhase<ErrorReportingSubsystem> errorReportingPhase = new(SubsystemKind.ErrorReporting);
    private readonly IEnumerable<PipelinePhase> phases;

    /// <summary>
    /// Creates an instance of the pipeline using standard features.
    /// </summary>
    /// <param name="help">A help subsystem to replace the standard one. To add a subsystem, use <see cref="AddSubsystem"></param>
    /// <param name="version">A help subsystem to replace the standard one. To add a subsystem, use <see cref="AddSubsystem"></param>
    /// <param name="completion">A help subsystem to replace the standard one. To add a subsystem, use <see cref="AddSubsystem"></param>
    /// <param name="diagram">A help subsystem to replace the standard one. To add a subsystem, use <see cref="AddSubsystem"></param>
    /// <param name="errorReporting">A help subsystem to replace the standard one. To add a subsystem, use <see cref="AddSubsystem"></param>
    /// <returns>A new pipeline.</returns>
    /// <remarks>
    /// Currently, the standard <see cref="ValueSubsystem"/>, <see cref="ValidationSubsystem"/> , and <see cref="ResponseSubsystem"/> cannot be replaced. <see cref="ResponseSubsystem"/> is disabled by default.
    /// </remarks>
    public static Pipeline Create(HelpSubsystem? help = null,
                                  VersionSubsystem? version = null,
                                  CompletionSubsystem? completion = null,
                                  DiagramSubsystem? diagram = null,
                                  ErrorReportingSubsystem? errorReporting = null)
        => new()
        {
            Help = help ?? new HelpSubsystem(),
            Version = version ?? new VersionSubsystem(),
            Completion = completion ?? new CompletionSubsystem(),
            Diagram = diagram ?? new DiagramSubsystem(),
            ErrorReporting = errorReporting ?? new ErrorReportingSubsystem(),
        };

    /// <summary>
    /// Creates an instance of the pipeline with no features. Use this if you want to explicitly add features.
    /// </summary>
    /// <returns>A new pipeline.</returns>
    /// <remarks>
    /// The <cref>ValueSubsystem</cref> and <see cref="ResponseSubsystem"/> is always added and cannot be changed.
    /// </remarks>
    public static Pipeline CreateEmpty()
        => new();

    private Pipeline()
    {
        Response = new ResponseSubsystem();
        invocationPhase.Subsystem = new InvocationSubsystem();
        validationPhase.Subsystem = new ValidationSubsystem();

        // This order is based on: if the user entered both, which should they get?
        // * It is reasonable to diagram help and completion. More reasonable than getting help on Diagram or Completion
        // * A future version of Help and Version may take arguments/options. In that case, help on version is reasonable.
        phases =
        [
            diagramPhase, completionPhase, helpPhase, versionPhase,
            validationPhase, invocationPhase, errorReportingPhase
        ];
    }

    /// <summary>
    /// Enables response files. They are disabled by default.
    /// </summary>
    public bool ResponseFilesEnabled
    {
        get => Response.Enabled;
        set => Response.Enabled = value;
    }

    /// <summary>
    /// Adds a subsystem.
    /// </summary>
    /// <param name="subsystem">The subsystem to add.</param>
    /// <param name="timing"><see cref="PhaseTiming.Before"/> indicates that the subsystem should run before all other subsystems in the phase, and <see cref="PhaseTiming.After"/> indicates it should run after other subsystems. The default is <see cref="PhaseTiming.Before"/>.</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>
    /// The phase in which the subsystem runs is determined by the subsystem's 'Kind' property.
    /// <br/>
    /// To replace one of the standard subsystems, use the `Pipeline.(subsystem)` property, such as `myPipeline.Help = new OtherHelp();`
    /// </remarks>
    public void AddSubsystem(CliSubsystem subsystem, AddToPhaseBehavior timing = AddToPhaseBehavior.SubsystemRecommendation)
    {
        switch (subsystem.Kind)
        {
            case SubsystemKind.Other:
            case SubsystemKind.Response:
            case SubsystemKind.Value:
                throw new InvalidOperationException($"You cannot add subsystems to {subsystem.Kind}");
            case SubsystemKind.Diagram:
                diagramPhase.AddSubsystem(subsystem, timing);
                break;
            case SubsystemKind.Completion:
                completionPhase.AddSubsystem(subsystem, timing);
                break;
            case SubsystemKind.Help:
                helpPhase.AddSubsystem(subsystem, timing);
                break;
            case SubsystemKind.Version:
                versionPhase.AddSubsystem(subsystem, timing);
                break;
            // You can add Validation and Invocation subsystems, but you can't remove the core.
            // Other things may need to be run in the phase.
            case SubsystemKind.Validation:
                validationPhase.AddSubsystem(subsystem, timing);
                break;
            case SubsystemKind.Invocation:
                invocationPhase.AddSubsystem(subsystem, timing);
                break;
            case SubsystemKind.ErrorReporting:
                errorReportingPhase.AddSubsystem(subsystem, timing);
                break;
        }
    }

    /// <summary>
    /// Sets or gets the diagramming subsystem.
    /// </summary>
    public DiagramSubsystem? Diagram
    {
        get => diagramPhase.Subsystem;
        set => diagramPhase.Subsystem = value;
    }

    /// <summary>
    /// Sets or gets the completion subsystem.
    /// </summary>
    public CompletionSubsystem? Completion
    {
        get => completionPhase.Subsystem;
        set => completionPhase.Subsystem = value;
    }

    /// <summary>
    /// Sets or gets the help subsystem.
    /// </summary>
    public HelpSubsystem? Help
    {
        get => helpPhase.Subsystem;
        set => helpPhase.Subsystem = value;
    }

    /// <summary>
    /// Sets or gets the version subsystem.
    /// </summary>
    public VersionSubsystem? Version
    {
        get => versionPhase.Subsystem;
        set => versionPhase.Subsystem = value;
    }

    /// <summary>
    /// Sets or gets the error reporting subsystem.
    /// </summary>
    public ErrorReportingSubsystem? ErrorReporting
    {
        get => errorReportingPhase.Subsystem;
        set => errorReportingPhase.Subsystem = value;
    }

    // TODO: Consider whether replacing the validation subsystem is valuable
    /// <summary>
    /// Sets or gets the validation subsystem
    /// </summary>
    public ValidationSubsystem? Validation => validationPhase.Subsystem;

    // TODO: Consider whether replacing the invocation subsystem is valuable
    /// <summary>
    /// Sets or gets the invocation subsystem
    /// </summary>
    public InvocationSubsystem? Invocation => invocationPhase.Subsystem;

    /// <summary>
    /// Gets the response file subsystem
    /// </summary>
    public ResponseSubsystem Response { get; }

    public ParseResult Parse(CliConfiguration configuration, string rawInput)
        => Parse(configuration, CliParser.SplitCommandLine(rawInput).ToArray());

    public ParseResult Parse(CliConfiguration configuration, IReadOnlyList<string> args)
    {
        InitializeSubsystems(new InitializationContext(configuration, args));
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        return parseResult;
    }

    public PipelineResult Execute(CliConfiguration configuration, string rawInput, ConsoleHack? consoleHack = null)
        => Execute(configuration, CliParser.SplitCommandLine(rawInput).ToArray(), rawInput, consoleHack);

    public PipelineResult Execute(CliConfiguration configuration, string[] args, string rawInput, ConsoleHack? consoleHack = null)
        => Execute(Parse(configuration, args), rawInput, consoleHack);

    public PipelineResult Execute(ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
    {
        var pipelineResult = new PipelineResult(parseResult, rawInput, this, consoleHack ?? new ConsoleHack());
        foreach (var phase in phases)
        {
            // TODO: Allow subsystems to control short-circuiting
            foreach (var subsystem in phase.GetSubsystems())
            {
                // TODO: RunEvenIfAlreadyHandled needs more thought and laying out the scenarios
                if (subsystem is not null && (!pipelineResult.AlreadyHandled || subsystem.RunsEvenIfAlreadyHandled))
                {
                    subsystem.ExecuteIfNeeded(pipelineResult);
                }
            }
        }
        TearDownSubsystems(pipelineResult);
        return pipelineResult;
    }

    // TODO: Consider whether this should be public. It would simplify testing, but would it do anything else
    // TODO: Confirm that it is OK for ConsoleHack to be unavailable in Initialize
    /// <summary>
    /// Perform any setup for the subsystem. This may include adding to the CLI definition,
    /// such as adding a help option. It is important that work only needed when the subsystem
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Note to inheritors: The ordering of initializing should normally be in the reverse order than tear down 
    /// </remarks>
    protected virtual void InitializeSubsystems(InitializationContext context)
    {
        foreach (var phase in phases)
        {
            // TODO: Allow subsystems to control short-circuiting? Not sure we need that for initialization
            foreach (var subsystem in phase.GetSubsystems())
            {
                subsystem.Initialize(context);
            }
        }
    }

    // TODO: Consider whether this should be public
    // TODO: Would Dispose be a better alternative? This may be non-dispose like things, such as removing options?
    /// <summary>
    /// Perform any cleanup operations
    /// </summary>
    /// <param name="pipelineResult">The context of the current execution</param>
    protected virtual void TearDownSubsystems(PipelineResult pipelineResult)
    {
        // TODO: Work on this design as the last pipelineResult wins and they may not all be well behaved
        foreach (var phase in phases)
        {
            // TODO: Allow subsystems to control short-circuiting? Not sure we need that for teardown
            foreach (var subsystem in phase.GetSubsystems())
            {
                subsystem.TearDown(pipelineResult);
            }
        }
    }
}
