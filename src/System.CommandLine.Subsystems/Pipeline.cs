﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

public class Pipeline
{
    public HelpSubsystem? Help { get; set; }
    public VersionSubsystem? Version { get; set; }
    public ErrorReportingSubsystem? ErrorReporting { get; set; }
    public CompletionSubsystem? Completion { get; set; }

    public ParseResult Parse(CliConfiguration configuration, string rawInput)
        => Parse(configuration, CliParser.SplitCommandLine(rawInput).ToArray());

    public ParseResult Parse(CliConfiguration configuration, string[] args)
    {
        InitializeSubsystems(configuration);
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        return parseResult;
    }

    public CliExit Execute(CliConfiguration configuration, string rawInput, ConsoleHack? consoleHack = null)
        => Execute(configuration, CliParser.SplitCommandLine(rawInput).ToArray(), rawInput, consoleHack);

    public CliExit Execute(CliConfiguration configuration, string[] args, string rawInput, ConsoleHack? consoleHack = null)
    {
        var cliExit = Execute(Parse(configuration, args), rawInput, consoleHack);
        return TearDownSubsystems(cliExit);
    }

    public CliExit Execute(ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
    {
        var pipelineContext = new PipelineContext(parseResult, rawInput, this, consoleHack ?? new ConsoleHack());
        ExecuteSubsystems(pipelineContext);
        return new CliExit(pipelineContext);
    }

    protected virtual void InitializeHelp(CliConfiguration configuration)
        => Help?.Initialize(configuration);

    protected virtual void InitializeVersion(CliConfiguration configuration)
        => Version?.Initialize(configuration);

    protected virtual void InitializeErrorReporting(CliConfiguration configuration)
        => ErrorReporting?.Initialize(configuration);

    protected virtual void InitializeCompletion(CliConfiguration configuration)
        => Completion?.Initialize(configuration);

    protected virtual CliExit TearDownHelp(CliExit cliExit)
        => Help is null
                ? cliExit
                : Help.TearDown(cliExit);

    protected virtual CliExit? TearDownVersion(CliExit cliExit)
        => Version is null
                ? cliExit
                : Version.TearDown(cliExit);

    protected virtual CliExit TearDownErrorReporting(CliExit cliExit)
        => ErrorReporting is null
                ? cliExit
                : ErrorReporting.TearDown(cliExit);

    protected virtual CliExit TearDownCompletions(CliExit cliExit)
        => Completion is null
                ? cliExit
                : Completion.TearDown(cliExit);

    protected virtual void ExecuteHelp(PipelineContext context)
        => ExecuteIfNeeded(Help, context);

    protected virtual void ExecuteVersion(PipelineContext context)
        => ExecuteIfNeeded(Version, context);

    protected virtual void ExecuteErrorReporting(PipelineContext context)
        => ExecuteIfNeeded(ErrorReporting, context);

    protected virtual void ExecuteCompletions(PipelineContext context)
        => ExecuteIfNeeded(Completion, context);

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
    protected virtual void InitializeSubsystems(CliConfiguration configuration)
    {
        InitializeHelp(configuration);
        InitializeVersion(configuration);
        InitializeErrorReporting(configuration);
        InitializeCompletion(configuration);
    }

    // TODO: Consider whether this should be public
    // TODO: Would Dispose be a better alternative? This may be non-dispose like things, such as removing options?
    /// <summary>
    /// Perform any cleanup operations
    /// </summary>
    /// <param name="pipelineContext">The context of the current execution</param>
    /// <remarks>
    /// Note to inheritors: The ordering of tear down should normally be in the reverse order than initializing
    /// </remarks>
    protected virtual CliExit TearDownSubsystems(CliExit cliExit)
    {
        TearDownCompletions(cliExit);
        TearDownErrorReporting(cliExit);
        TearDownVersion(cliExit);
        TearDownHelp(cliExit);
        return cliExit;
    }

    protected virtual void ExecuteSubsystems(PipelineContext pipelineContext)
    {
        ExecuteHelp(pipelineContext);
        ExecuteVersion(pipelineContext);
        ExecuteErrorReporting(pipelineContext);
        ExecuteCompletions(pipelineContext);
    }

    protected static void ExecuteIfNeeded(CliSubsystem? subsystem, PipelineContext pipelineContext)
    {
        if (subsystem is not null && (!pipelineContext.AlreadyHandled || subsystem.RunsEvenIfAlreadyHandled))
        {
            subsystem.ExecuteIfNeeded(pipelineContext);
        }
    }

}
