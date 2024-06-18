// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Directives;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine.Subsystems.Tests
{
    internal class AlternateSubsystems
    {
        internal class AlternateVersion : VersionSubsystem
        {
            protected override void Execute(PipelineResult pipelineResult)
            {
                pipelineResult.ConsoleHack.WriteLine($"***{CliExecutable.ExecutableVersion}***");
                pipelineResult.SetSuccess();
            }
        }

        internal class AlternateHelp : HelpSubsystem
        {
            protected override void Execute(PipelineResult pipelineResult)
            {
                pipelineResult.ConsoleHack.WriteLine("***Help me!***");
                pipelineResult.SetSuccess();
            }
        }

        internal class VersionThatUsesHelpData : VersionSubsystem
        {
            // for testing, this class accepts a symbol and accesses its description

            public VersionThatUsesHelpData(CliSymbol symbol)
            {
                Symbol = symbol;
            }

            private CliSymbol Symbol { get; }

            protected override void Execute(PipelineResult pipelineResult)
            {
                TryGetAnnotation(Symbol, HelpAnnotations.Description, out string? description);
                pipelineResult.ConsoleHack.WriteLine(description);
                pipelineResult.AlreadyHandled = true;
                pipelineResult.SetSuccess();
            }
        }

        internal class VersionWithInitializeAndTeardown : VersionSubsystem
        {
            internal bool InitializationWasRun;
            internal bool ExecutionWasRun;
            internal bool TeardownWasRun;

            protected override void Initialize(InitializationContext context)
            {
                base.Initialize(context);
                // marker hack needed because ConsoleHack not available in initialization
                InitializationWasRun = true;
            }

            protected override void Execute(PipelineResult pipelineResult)
            {
                ExecutionWasRun = true;
                base.Execute(pipelineResult);
            }

            protected override void TearDown(PipelineResult pipelineResult)
            {
                TeardownWasRun = true;
                base.TearDown(pipelineResult);
            }
        }

        internal class StringDirectiveSubsystem(IAnnotationProvider? annotationProvider = null)
           : DirectiveSubsystem("other", SubsystemKind.Diagram, annotationProvider)
        { }

        internal class BooleanDirectiveSubsystem(IAnnotationProvider? annotationProvider = null)
           : DirectiveSubsystem("diagram", SubsystemKind.Diagram, annotationProvider)
        { }

    }
}
