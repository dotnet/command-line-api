// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;

namespace System.CommandLine.Subsystems.Tests
{
    internal class AlternateSubsystems
    {
        internal class Version : VersionSubsystem
        {
            protected override CliExit Execute(PipelineContext pipelineContext)
            {
                pipelineContext.ConsoleHack.WriteLine($"***{CliExecutable.ExecutableVersion}***");
                pipelineContext.AlreadyHandled = true;
                return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
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

            protected override CliExit Execute(PipelineContext pipelineContext)
            {
                var help = pipelineContext.Pipeline.Help ?? throw new InvalidOperationException("Help cannot be null for this subsystem to work");
                var data = help.GetDescription(Symbol);

                pipelineContext.ConsoleHack.WriteLine(data);
                pipelineContext.AlreadyHandled = true;
                return CliExit.SuccessfullyHandled(pipelineContext.ParseResult);
            }
        }

    }
}
