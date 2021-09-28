// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="IConsole" />.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Writes the current string value to the standard output stream.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this IConsole console, string value) =>
            console.Out.Write(value);

        /// <summary>
        /// Writes the current string value, followed by the current environment's line terminator, to the standard output stream.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IConsole console, string value) =>
            console.Out.WriteLine(value);
    }
}