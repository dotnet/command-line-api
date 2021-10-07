// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    /// <summary>
    /// Represents a console's standard error stream.
    /// </summary>
    public interface IStandardError
    {
        /// <summary>
        /// The stream writer for standard error.
        /// </summary>
        IStandardStreamWriter Error { get; }

        /// <summary>
        /// Indicates whether the standard error stream has been redirected.
        /// </summary>
        bool IsErrorRedirected { get; }
    }
}