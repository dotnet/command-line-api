// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Supports formatting command line help.
    /// </summary>
    public class HelpContext
    {
        /// <param name="helpBuilder">The current help builder.</param>
        /// <param name="command">The command for which help is being formatted.</param>
        /// <param name="output">A text writer to write output to.</param>
        /// <param name="parseResult">The result of the current parse operation.</param>
        public HelpContext(
            HelpBuilder helpBuilder,
            Command command,
            TextWriter output,
            ParseResult? parseResult = null)
        {
            HelpBuilder = helpBuilder ?? throw new ArgumentNullException(nameof(helpBuilder));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Output = output ?? throw new ArgumentNullException(nameof(output));
            ParseResult = parseResult ?? ParseResult.Empty();
        }

        /// <summary>
        /// The help builder for the current operation.
        /// </summary>
        public HelpBuilder HelpBuilder { get; }

        /// <summary>
        /// The result of the current parse operation.
        /// </summary>
        public ParseResult ParseResult { get; }

        /// <summary>
        /// The command for which help is being formatted.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// A text writer to write output to.
        /// </summary>
        public TextWriter Output { get; }

        internal bool WasSectionSkipped { get; set; }
    }
}