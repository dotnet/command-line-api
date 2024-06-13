// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

// stub Help subsystem demonstrating annotation model.
//
// usage:
//
//
//        var help = new HelpSubsystem();
//        var command = new CliCommand("greet")
//          .With(help.Description, "Greet the user");
//
public class HelpSubsystem(IAnnotationProvider? annotationProvider = null)
    : CliSubsystem(HelpAnnotations.Prefix, SubsystemKind.Help, annotationProvider)
{
    public CliOption<bool> HelpOption { get; } = new CliOption<bool>("--help", ["-h"])
    {
        // TODO: Why don't we accept bool like any other bool option?
        Arity = ArgumentArity.Zero
    };

    protected internal override void Initialize(InitializationContext context)
    {
        AddOptionRecursively(context.Configuration.RootCommand, HelpOption);

        // I imagine this method should belong to CliCommand
        // or some extensions method, but I didn't want to change
        // too much in this PR without consulting you first
        static void AddOptionRecursively(CliCommand command, CliOption option)
        {
            command.Add(option);

            foreach (var subcommand in command.Subcommands)
            {
                AddOptionRecursively(subcommand, option);
            }
        }
    }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.GetValue(HelpOption);

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        // TODO: Match testable output pattern
        pipelineResult.ConsoleHack.WriteLine("Help me!");
        pipelineResult.SetSuccess();
    }

    public bool TryGetDescription (CliSymbol symbol, out string? description)
        => TryGetAnnotation (symbol, HelpAnnotations.Description, out description);
}
