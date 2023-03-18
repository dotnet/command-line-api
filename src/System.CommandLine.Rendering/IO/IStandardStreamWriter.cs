// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    /// <summary>
    /// Represents a standard stream that can be written to.
    /// </summary>
    public interface IStandardStreamWriter
    {
        /// <summary>
        /// Writes the specified string to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void Write(string? value);
    }
}