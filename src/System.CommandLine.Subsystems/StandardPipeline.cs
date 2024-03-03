// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

public class StandardPipeline : Pipeline
{ 
    public StandardPipeline() {
        Help = new HelpSubsystem();
        Version = new VersionSubsystem();
        ErrorReporting = new ErrorReportingSubsystem();
        Completion = new CompletionSubsystem();
    }
}
