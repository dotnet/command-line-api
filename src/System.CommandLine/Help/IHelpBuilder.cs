// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Formats output to be shown to users to describe how to use a command line tool.
    /// </summary>
    public interface IHelpBuilder
    {
        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        /// <param name="command">The command for which to write help output.</param>
        /// <param name="output">The output stream.</param>
        void Write(ICommand command, TextWriter output);
    }
}