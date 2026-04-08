// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

/// <summary>
/// Specifies when a value factory should be invoked.
/// </summary>
[Flags]
public enum ValueFactoryInvocation
{
    /// <summary>
    /// Invoke the value factory when tokens were provided for the argument.
    /// </summary>
    WhenTokensMatched = 1,

    /// <summary>
    /// Invoke the value factory when no tokens were provided for the argument.
    /// </summary>
    WhenTokensNotMatched = 2,

    /// <summary>
    /// Invoke the value factory whether or not tokens were provided for the argument.
    /// </summary>
    Always = WhenTokensMatched | WhenTokensNotMatched
}