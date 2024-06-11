// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public class Subsystem
{
    public static void Initialize(CliSubsystem subsystem, CliConfiguration configuration, IReadOnlyList<string> args)
        => subsystem.Initialize(new InitializationContext(configuration, args));

    public static void Execute(CliSubsystem subsystem, PipelineResult pipelineResult)
        => subsystem.Execute(pipelineResult);

    public static bool GetIsActivated(CliSubsystem subsystem, ParseResult parseResult)
        => subsystem.GetIsActivated(parseResult);

    public static PipelineResult ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
        => subsystem.ExecuteIfNeeded(new PipelineResult(parseResult, rawInput, null, consoleHack));

    public static PipelineResult Execute(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack = null)
    {
        var pipelineResult = new PipelineResult(parseResult, rawInput,null, consoleHack);
        subsystem.Execute(pipelineResult);
        return pipelineResult;
    }

    internal static PipelineResult ExecuteIfNeeded(CliSubsystem subsystem, ParseResult parseResult, string rawInput, ConsoleHack? consoleHack, PipelineResult? pipelineResult = null)
    {
        pipelineResult ??= new PipelineResult(parseResult, rawInput, null, consoleHack);
        subsystem.ExecuteIfNeeded(pipelineResult );
        return pipelineResult;
    }

    internal static void ExecuteIfNeeded(CliSubsystem subsystem, PipelineResult pipelineResult)
        => subsystem.ExecuteIfNeeded(pipelineResult);
}
