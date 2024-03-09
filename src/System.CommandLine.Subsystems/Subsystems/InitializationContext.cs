// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

public class InitializationContext(CliConfiguration configuration, IReadOnlyList<string> args)
{
    public CliConfiguration Configuration { get; } = configuration;
    public IReadOnlyList<string> Args { get; } = args;
}
