// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public class Subsystem
{
    public static void Initialize(CliSubsystem subsystem, CliConfiguration configuration, IReadOnlyList<string> args)
        => subsystem.Initialize(new InitializationContext(configuration, args));

    public static CliExit Execute(CliSubsystem subsystem, PipelineResult pipelineResult)
        => subsystem.Execute(pipelineResult);

    public static bool GetIsActivated(CliSubsystem subsystem, ParseResult parseResult)
        => subsystem.GetIsActivated(parseResult);

    public static CliExit ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
        => new(subsystem.ExecuteIfNeeded(new PipelineResult(parseResult, rawInput, null, consoleHack)));

    public static CliExit Execute(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
        => subsystem.Execute(new PipelineResult(parseResult, rawInput, null, consoleHack));


    internal static PipelineResult ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack, PipelineResult? pipelineResult = null)
        => subsystem.ExecuteIfNeeded(pipelineResult ?? new PipelineResult(parseResult, rawInput, null, consoleHack));

    internal static PipelineResult ExecuteIfNeeded(CliSubsystem subsystem, PipelineResult pipelineResult)
        => subsystem.ExecuteIfNeeded(pipelineResult);
}
