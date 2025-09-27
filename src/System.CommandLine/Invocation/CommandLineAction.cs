// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation;

/// <summary>
/// Defines a behavior associated with a command line symbol.
/// </summary>
public abstract class CommandLineAction
{
    private protected CommandLineAction()
    {
    }

    /// <summary>
    /// Indicates that the action terminates a command line invocation, and later actions are skipped.
    /// </summary>
    public virtual bool Terminating => true;

    /// <summary>
    /// Indicates that the action clears any parse errors associated with symbols other than one that owns the <see cref="CommandLineAction"/>.
    /// </summary>
    /// <remarks>This property is ignored when <see cref="Terminating"/> is set to <see langword="false"/>.</remarks>
    public virtual bool ClearsParseErrors => false;
}