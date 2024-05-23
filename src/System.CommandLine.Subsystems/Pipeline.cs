// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

public class Pipeline
{
    //TODO:  When we allow adding subsystems, this code will change
    private IEnumerable<CliSubsystem?> Subsystems
        => [Help, Version, Completion, Diagram, Value, ErrorReporting];

    public static Pipeline Create(HelpSubsystem? help = null,
                                  VersionSubsystem? version = null,
                                  CompletionSubsystem? completion = null,
                                  DiagramSubsystem? diagram = null,
                                  ErrorReportingSubsystem? errorReporting = null,
                                  ValueSubsystem? value = null)
        => new()
        {
            Help = help ?? new HelpSubsystem(),
            Version = version ?? new VersionSubsystem(),
            Completion = completion ?? new CompletionSubsystem(),
            Diagram = diagram ?? new DiagramSubsystem(),
            ErrorReporting = errorReporting ?? new ErrorReportingSubsystem(),
            Value = value ?? new ValueSubsystem()
        };

    public static Pipeline CreateEmpty()
        => new();

    private Pipeline() { }

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
    {
        var pipelineResult = Execute(Parse(configuration, args), rawInput, consoleHack);
        return TearDownSubsystems(pipelineResult);
    }

    public PipelineResult Execute(ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
    {
        var pipelineResult = new PipelineResult(parseResult, rawInput, this, consoleHack ?? new ConsoleHack());
        ExecuteSubsystems(pipelineResult);
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
        foreach (var subsystem in Subsystems)
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
    protected virtual PipelineResult TearDownSubsystems(PipelineResult pipelineResult)
    {
        // TODO: Work on this design as the last pipelineResult wins and they may not all be well behaved
        var subsystems = Subsystems.Reverse();
        foreach (var subsystem in subsystems)
        {
            if (subsystem is not null)
            {
                pipelineResult = subsystem.TearDown(pipelineResult);
            }
        }
        return pipelineResult;
    }

    protected virtual void ExecuteSubsystems(PipelineResult pipelineResult)
    {
        // TODO: Consider redesign where pipelineResult is not modifiable. 
        // 
        foreach (var subsystem in Subsystems)
        {
            if (subsystem is not null)
            {
                pipelineResult = subsystem.ExecuteIfNeeded(pipelineResult);
            }
        }
    }

    protected static void ExecuteIfNeeded(CliSubsystem? subsystem, PipelineResult pipelineResult)
    {
        if (subsystem is not null && (!pipelineResult.AlreadyHandled || subsystem.RunsEvenIfAlreadyHandled))
        {
            subsystem.ExecuteIfNeeded(pipelineResult);
        }
    }

}
