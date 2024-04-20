// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

public class CommandValueResult
{
    public CommandValueResult(CliCommand command, CommandValueResult parent)
    {
        Command = command;
        Parent = parent;
    }
    public IEnumerable<ValueResult> ValueResults { get; } = new List<ValueResult>();
    public CliCommand Command { get; }
    public CommandValueResult Parent { get; }

}
