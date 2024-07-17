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
    /// <summary>
    /// Gets the help option, which allows the user to customize
    /// </summary>
    /// <remarks>
    /// By design, the user can modify the help option but not replace it. This allows us to 
    /// do the fastest possible lookup of whether help was called, and we aren't clear why 
    /// the option would need to be replaced
    /// </remarks>
    public CliOption<bool> HelpOption { get; } = new CliOption<bool>("--help", ["-h"])
    {
        // TODO: Why don't we accept bool like any other bool option?
        Arity = ArgumentArity.Zero
    };

    protected internal override void Initialize(InitializationContext context)
        => context.Configuration.RootCommand.Add(HelpOption);

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.GetValue(HelpOption);

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        // TODO: Match testable output pattern
        pipelineResult.ConsoleHack.WriteLine("Help me!");
        pipelineResult.SetSuccess();
    }

    public bool TryGetDescription(CliSymbol symbol, out string? description)
        => TryGetAnnotation(symbol, HelpAnnotations.Description, out description);
}
