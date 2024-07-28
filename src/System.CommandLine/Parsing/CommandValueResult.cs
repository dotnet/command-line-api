// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

/// <summary>
/// Provides the publicly facing command result
/// </summary>
/// <remarks>
/// The name is temporary as we expect to later name this CommandResult and the previous one to CommandResultInternal
/// </remarks>
public class CommandValueResult
{
    /// <summary>
    /// Creates a CommandValueResult instance
    /// </summary>
    /// <param name="commandValueResult">The CommandResult used to build the CommandValueResult.</param>
    /// <param name="parent">The parent command in the case of a CLI hierarchy, or null if there is no parent.</param>
    internal CommandValueResult(CommandResult commandValueResult, CommandResult? parent = null)
    {
        Command = commandValueResult.Command;
        ValueResults = commandValueResult.ValueResults;
        if (parent is not null)
        {
            Parent = new CommandValueResult(parent);
        }
    }

    /// <summary>
    /// The ValueResult instances for user entered data. This is a sparse list.
    /// </summary>
    public IEnumerable<ValueResult> ValueResults { get; } = new List<ValueResult>();

    /// <summary>
    /// The CliCommand that the result is for. 
    /// </summary>
    public CliCommand Command { get; }

    /// <summary>
    /// The command's parent if one exists, otherwise, null
    /// </summary>
    public CommandValueResult? Parent { get; }

}
