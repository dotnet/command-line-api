// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Supports formatting command line help.
    /// </summary>
    internal class HelpContext
    {
        /// <param name="helpBuilder">The current help builder.</param>
        /// <param name="command">The command for which help is being formatted.</param>
        /// <param name="output">A text writer to write output to.</param>
        public HelpContext(
            HelpBuilder helpBuilder,
            Command command,
            TextWriter output)
        {
            HelpBuilder = helpBuilder ?? throw new ArgumentNullException(nameof(helpBuilder));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// The help builder for the current operation.
        /// </summary>
        public HelpBuilder HelpBuilder { get; }

        /// <summary>
        /// The command for which help is being formatted.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// A text writer to write output to.
        /// </summary>
        public TextWriter Output { get; }
    }
}