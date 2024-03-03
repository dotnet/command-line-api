// Copyright (c) .NET Foundation and contributors. All rights reserved.
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

    protected virtual void InitializeHelp(CliConfiguration configuration)
        => Help?.Initialize(configuration);

    protected virtual void InitializeVersion(CliConfiguration configuration)
        => Version?.Initialize(configuration);

    protected virtual void InitializeErrorReporting(CliConfiguration configuration)
        => ErrorReporting?.Initialize(configuration);

    protected virtual void InitializeCompletions(CliConfiguration configuration)
        => Completion?.Initialize(configuration);

    protected virtual void TearDownHelp(PipelineContext context)
        => Help?.TearDown(context);

    protected virtual void TearDownVersion(PipelineContext context)
        => Version?.TearDown(context);

    protected virtual void TearDownErrorReporting(PipelineContext context)
        => ErrorReporting?.TearDown(context);

    protected virtual void TearDownCompletions(PipelineContext context)
        => Completion?.TearDown(context);

    protected virtual void ExecuteHelp(PipelineContext context)
        => ExecuteIfNeeded(Help, context);

    protected virtual void ExecuteVersion(PipelineContext context)
        => ExecuteIfNeeded(Version, context);

    protected virtual void ExecuteErrorReporting(PipelineContext context)
        => ExecuteIfNeeded(ErrorReporting, context);

    protected virtual void ExecuteCompletions(PipelineContext context)
        => ExecuteIfNeeded(Completion, context);

    protected static void ExecuteIfNeeded(CliSubsystem? subsystem, PipelineContext pipelineContext)
    {
        if (subsystem is not null && (!pipelineContext.AlreadyHandled || subsystem.RunsEvenIfAlreadyHandled))
        {
            subsystem.ExecuteIfNeeded(pipelineContext);
        }
    }

    public virtual void InitializeExtensions(CliConfiguration configuration)
    {
        InitializeHelp(configuration);
        InitializeVersion(configuration);
        InitializeErrorReporting(configuration);
        InitializeCompletions(configuration);
    }

    public virtual void TearDownExtensions(PipelineContext pipelineContext)
    {
        TearDownHelp(pipelineContext);
        TearDownVersion(pipelineContext);
        TearDownErrorReporting(pipelineContext);
        TearDownCompletions(pipelineContext);
    }

    protected virtual void ExecuteRequestedExtensions(PipelineContext pipelineContext)
    {
        ExecuteHelp(pipelineContext);
        ExecuteVersion(pipelineContext);
        ExecuteErrorReporting(pipelineContext);
        ExecuteCompletions(pipelineContext);
    }

    public ParseResult Parse(CliConfiguration configuration, string rawInput)
        => Parse(configuration, CliParser.SplitCommandLine(rawInput).ToArray());

    public ParseResult Parse(CliConfiguration configuration, string[] args)
    {
        InitializeExtensions(configuration);
        var parseResult = CliParser.Parse(configuration.RootCommand, args, configuration);
        return parseResult;
    }

    public CliExit Execute(CliConfiguration configuration, string rawInput, ConsoleHack? consoleHack = null)
        => Execute(configuration, CliParser.SplitCommandLine(rawInput).ToArray(), rawInput, consoleHack);

    public CliExit Execute(CliConfiguration configuration, string[] args, string rawInput, ConsoleHack? consoleHack = null)
        => Execute(Parse(configuration, args), rawInput, consoleHack);

    public CliExit Execute(ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
    {
        var pipelineContext = new PipelineContext(parseResult, rawInput, this, consoleHack ?? new ConsoleHack());
        ExecuteRequestedExtensions(pipelineContext);
        return new CliExit(pipelineContext);
    }
}
