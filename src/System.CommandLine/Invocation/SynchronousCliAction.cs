// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation;

/// <summary>
/// Defines a synchronous behavior associated with a command line symbol.
/// </summary>
public abstract class SynchronousCliAction : CliAction
{
    /// <summary>
    /// Performs an action when the associated symbol is invoked on the command line.
    /// </summary>
    /// <param name="parseResult">Provides the parse results.</param>
    /// <returns>A value that can be used as the exit code for the process.</returns>
    public abstract int Invoke(ParseResult parseResult);
}