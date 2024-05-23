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
            protected override PipelineResult Execute(PipelineResult pipelineResult)
            {
                pipelineResult.ConsoleHack.WriteLine($"***{CliExecutable.ExecutableVersion}***");
                pipelineResult.SetSuccess();
                return pipelineResult;
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

            protected override PipelineResult Execute(PipelineResult pipelineResult)
            {
                TryGetAnnotation(Symbol, HelpAnnotations.Description, out string? description);
                pipelineResult.ConsoleHack.WriteLine(description);
                pipelineResult.AlreadyHandled = true;
                pipelineResult.SetSuccess();
                return pipelineResult;
            }
        }

        internal class VersionWithInitializeAndTeardown : VersionSubsystem
        {
            internal bool InitializationWasRun;
            internal bool ExecutionWasRun;
            internal bool TeardownWasRun;

            protected override CliConfiguration Initialize(InitializationContext context)
            {
                // marker hack needed because ConsoleHack not available in initialization
                InitializationWasRun = true;
                return base.Initialize(context);
            }

            protected override PipelineResult Execute(PipelineResult pipelineResult)
            {
                ExecutionWasRun = true;
                return base.Execute(pipelineResult);
            }

            protected override PipelineResult TearDown(PipelineResult pipelineResult)
            {
                TeardownWasRun = true;
                return base.TearDown(pipelineResult);
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
