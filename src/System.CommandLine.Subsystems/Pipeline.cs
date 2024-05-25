// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

public partial class Pipeline
{
    //TODO:  When we allow adding subsystems, this code will change
    private readonly Subsystems subsystems = new();

    public static Pipeline Create(HelpSubsystem? help = null,
                                  VersionSubsystem? version = null,
                                  CompletionSubsystem? completion = null,
                                  DiagramSubsystem? diagram = null,
                                  ErrorReportingSubsystem? errorReporting = null,
                                  ValueSubsystem? value = null)
    {
        Pipeline pipeline = new()
        {
            Help = help ?? new HelpSubsystem(),
            Version = version ?? new VersionSubsystem(),
            Completion = completion ?? new CompletionSubsystem(),
            Diagram = diagram ?? new DiagramSubsystem(),
            ErrorReporting = errorReporting ?? new ErrorReportingSubsystem(),
            Value = value ?? new ValueSubsystem()
        };
        // This order is based on: if the user entered both, which should they get?
        // * It is reasonable to diagram help and completion. More reasonable than getting help on Diagram or Completion
        // * A future version of Help and Version may take arguments/options. In that case, help on version is reasonable.
        pipeline.AddSubsystem(pipeline.Diagram);
        pipeline.AddSubsystem(pipeline.Completion);
        pipeline.AddSubsystem(pipeline.Help);
        pipeline.AddSubsystem(pipeline.Version);
        //pipeline.AddSubsystem(pipeline.Value);
        pipeline.AddSubsystem(pipeline.ErrorReporting);

        return pipeline;
    }

    public static Pipeline CreateEmpty()
        => new();

    private Pipeline() { }

    public void AddSubsystem(CliSubsystem? subsystem, bool insertAtStart = false)
        => subsystems.Add(subsystem, insertAtStart);

    public void InsertSubsystemAfter(CliSubsystem? subsystem, CliSubsystem existingSubsystem)
        => subsystems.Insert(subsystem, existingSubsystem);

    public void InsertSubsystemBefore(CliSubsystem? subsystem, CliSubsystem existingSubsystem)
        => subsystems.Insert(subsystem, existingSubsystem, true);

    public IEnumerable<CliSubsystem> EarlyReturnSubsystems
        => subsystems.EarlyReturnSubsystems;

    public IEnumerable<CliSubsystem> ValidationSubsystems
        => subsystems.ValidationSubsystems;

    public IEnumerable<CliSubsystem> ExecutionSubsystems
        => subsystems.ExecutionSubsystems;

    public IEnumerable<CliSubsystem> FinishSubsystems
        => subsystems.FinishSubsystems;


    public HelpSubsystem? Help { get; set; }
    public VersionSubsystem? Version { get; set; }
    public CompletionSubsystem? Completion { get; set; }
    public DiagramSubsystem? Diagram { get; set; }
    public ErrorReportingSubsystem? ErrorReporting { get; set; }
    public ValueSubsystem? Value { get; set; }

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
        ExecuteSubsystems(EarlyReturnSubsystems, pipelineResult);
        ExecuteSubsystems(ValidationSubsystems, pipelineResult);
        ExecuteSubsystems(ExecutionSubsystems, pipelineResult);
        ExecuteSubsystems(FinishSubsystems, pipelineResult);
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
        foreach (var subsystem in subsystems)
        {
            if (subsystem is not null)
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
        var subsystems = this.subsystems.Reverse<CliSubsystem>();
        foreach (var subsystem in subsystems)
        {
            if (subsystem is not null)
            {
                subsystem.TearDown(pipelineResult);
            }
        }
    }

    private static void ExecuteSubsystems(IEnumerable<CliSubsystem> subsystems, PipelineResult pipelineResult)
    {
        foreach (var subsystem in subsystems)
        {
            if (subsystem is not null && (!pipelineResult.AlreadyHandled || subsystem.RunsEvenIfAlreadyHandled))
            {
                subsystem.ExecuteIfNeeded(pipelineResult);
            }
        }
    }
}
