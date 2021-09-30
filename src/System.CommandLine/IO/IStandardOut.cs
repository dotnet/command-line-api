// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    /// <summary>
    /// Represents a console's standard output stream.
    /// </summary>
    public interface IStandardOut
    {
        /// <summary>
        /// The stream writer for standard output.
        /// </summary>
        IStandardStreamWriter Out { get; }

        /// <summary>
        /// Indicates whether the standard output stream has been redirected.
        /// </summary>
        bool IsOutputRedirected { get; }
    }
}