// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace System.CommandLine;

/// <summary>
/// Indicates that a command line configuration is invalid.
/// </summary>
[Serializable]
public class CommandLineConfigurationException : Exception
{
    /// <inheritdoc />
    public CommandLineConfigurationException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public CommandLineConfigurationException()
    {
    }

    /// <inheritdoc />
    protected CommandLineConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    public CommandLineConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}