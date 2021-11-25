// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Supports formatting command line help.
    /// </summary>
    public class HelpContext
    {
        /// <param name="helpBuilder">The current help builder.</param>
        /// <param name="parseResult">The result of the current parse operation.</param>
        /// <param name="command">The command for which help is being formatted.</param>
        public HelpContext(HelpBuilder helpBuilder, ParseResult parseResult, ICommand command)
        {
            HelpBuilder = helpBuilder;
            ParseResult = parseResult;
            Command = command;
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
    }

    /// <summary>
    /// Specifies help formatting behavior.
    /// </summary>
    public delegate string? HelpDelegate(HelpContext context);
}