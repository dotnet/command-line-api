// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public class Subsystem
{
    public static void Initialize(CliSubsystem subsystem, CliConfiguration configuration)
        => subsystem.Initialize(configuration);

    public static CliExit Execute(CliSubsystem subsystem, PipelineContext pipelineContext)
        => subsystem.Execute(pipelineContext);

    public static bool GetIsActivated(CliSubsystem subsystem, ParseResult parseResult)
        => subsystem.GetIsActivated(parseResult);

    public static CliExit ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null) 
        => new(subsystem.ExecuteIfNeeded(new PipelineContext(parseResult,  rawInput, null,consoleHack)));

    internal static PipelineContext ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack, PipelineContext? pipelineContext = null) 
        => subsystem.ExecuteIfNeeded(pipelineContext ?? new PipelineContext(parseResult, rawInput, null,consoleHack));

    internal static PipelineContext ExecuteIfNeeded(CliSubsystem subsystem, PipelineContext pipelineContext)
        => subsystem.ExecuteIfNeeded(pipelineContext);
}
