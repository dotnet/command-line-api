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

    protected internal override CliConfiguration Initialize(InitializationContext context)
    {
        context.Configuration.RootCommand.Add(HelpOption);

        return context.Configuration;
    }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.GetValue(HelpOption);

    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        // TODO: Match testable output pattern
        pipelineContext.ConsoleHack.WriteLine("Help me!");
        return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
    }
}
