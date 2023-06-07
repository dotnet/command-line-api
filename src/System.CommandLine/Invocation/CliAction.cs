// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation;

/// <summary>
/// Defines a behavior associated with a command line symbol.
/// </summary>
public abstract class CliAction
{
    private protected CliAction()
    {
    }

    /// <summary>
    /// Indicates that the action terminates a command line invocation, and later actions are skipped.
    /// </summary>
    public bool Terminating { get; protected init; } = true;
}