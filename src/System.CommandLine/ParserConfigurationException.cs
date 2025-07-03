// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

/// <summary>
/// Indicates that a command line configuration is invalid.
/// </summary>
public class ParserConfigurationException : Exception
{
    /// <inheritdoc />
    public ParserConfigurationException(string message) : base(message)
    {
    }
}