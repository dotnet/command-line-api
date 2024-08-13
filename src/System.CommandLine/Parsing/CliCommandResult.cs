// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

/// <summary>
/// Provides the publicly facing command result
/// </summary>
/// <remarks>
/// The name is temporary as we expect to later name this CliCommandResultInternal and the previous one to CommandResultInternal
/// </remarks>
public class CliCommandResult : CliSymbolResult
{
    /// <summary>
    /// Creates a CommandValueResult instance
    /// </summary>
    /// <param name="command">The CliCommand that the result is for.</param>
    /// <param name="locations"></param>
    /// <param name="parent">The parent command in the case of a CLI hierarchy, or null if there is no parent.</param>
    internal CliCommandResult(
        CliCommand command,
        IEnumerable<Location> locations,
        CliCommandResult? parent = null)
        : base(locations)
    {
        Command = command;
        Parent = parent;
    }

    /// <summary>
    /// The ValueResult instances for user entered data. This is a sparse list.
    /// </summary>
    public IEnumerable<CliValueResult> ValueResults { get; } = new List<CliValueResult>();

    /// <summary>
    /// The CliCommand that the result is for. 
    /// </summary>
    public CliCommand Command { get; }

    /// <summary>
    /// The command's parent if one exists, otherwise, null
    /// </summary>
    public CliCommandResult? Parent { get; }

}
