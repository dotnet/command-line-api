// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;
using System.Reflection;

namespace System.CommandLine;

public class VersionSubsystem : CliSubsystem
{
    private string? specificVersion = null;

    public VersionSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(VersionAnnotations.Prefix, annotationProvider, SubsystemKind.Version)
    {
    }

    // TODO: Should we block adding version anywhere but root?
    public string? SpecificVersion
    {
        get
        {
            var version = specificVersion is null
                ? AssemblyVersion(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                : specificVersion;
            return version ?? "";
        }
        set => specificVersion = value;
    }

    public static string? AssemblyVersion(Assembly assembly) 
        => assembly
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;


    protected internal override CliConfiguration Initialize(CliConfiguration configuration)
    {
        var option = new CliOption<bool>("--version", ["-v"])
        {
            Arity = ArgumentArity.Zero
        };
        configuration.RootCommand.Add(option);

        return configuration;
    }

    // TODO: Stash option rather than using string
    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => parseResult is not null && parseResult.GetValue<bool>("--version");

    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        var subsystemVersion = SpecificVersion;
        var version = subsystemVersion is null
            ? CliExecutable.ExecutableVersion
            : subsystemVersion;
        pipelineContext.ConsoleHack.WriteLine(version);
        pipelineContext.AlreadyHandled = true;
        return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
    }
}

