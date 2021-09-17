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
        public static void Write(this IConsole console, string value) => 
            console.Out.Write(value);

        public static void WriteLine(this IConsole console, string value) => 
            console.Out.WriteLine(value);
    }
}