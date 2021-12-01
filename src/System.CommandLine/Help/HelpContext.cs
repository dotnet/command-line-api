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
        /// <param name="maxWidth">The maximum width at which to format the text, used for wrapping text within multi-column output.</param>
        public HelpContext(
            HelpBuilder helpBuilder,
            ICommand command,
            TextWriter output,
            ParseResult? parseResult = null,
            int? maxWidth = int.MaxValue)
        {
            HelpBuilder = helpBuilder;
            ParseResult = parseResult ?? ParseResult.Empty();
            Command = command;
            Output = output;
            if (maxWidth is not null)
            {
                MaxWidth = maxWidth.Value;
            }
            else
            {
                MaxWidth = int.MaxValue;
            }
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
        public ICommand Command { get; }

        /// <summary>
        /// A text writer to write output to.
        /// </summary>
        public TextWriter Output { get; }

        /// <summary>
        /// The maximum width at which to format the text, used for wrapping text within multi-column output.
        /// </summary>
        public int MaxWidth { get; }
    }
}