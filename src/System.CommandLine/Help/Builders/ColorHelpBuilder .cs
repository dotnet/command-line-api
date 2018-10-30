// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ColorHelpBuilder : HelpBuilder
    {
        protected const ConsoleColor DefaultHeadingColor = ConsoleColor.Cyan;

        public ConsoleColor HeadingColor { get; } = DefaultHeadingColor;

        /// <inheritdoc />
        /// <summary>
        /// Unlike the base <see cref="HelpBuilder"/>, this derivation sets the color of the heading texdt
        /// to the configured <see cref="HeadingColor"/>
        /// </summary>
        /// <param name="console"><see cref="IConsole"/> instance to write the help text output</param>
        /// <param name="headingColor">
        /// <see cref="ConsoleColor"/> to apply to the headings written to the console
        /// </param>
        /// <param name="columnGutter">
        /// Number of characters to pad invocation information from their descriptions
        /// </param>
        /// <param name="indentationSize">Number of characters to indent new lines</param>
        /// <param name="maxWidth">
        /// Maximum number of characters available for each line to write to the console
        /// </param>
        public ColorHelpBuilder(
            IConsole console,
            ConsoleColor? headingColor = null,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
            HeadingColor = headingColor ?? DefaultHeadingColor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Colorizes the headins to the configured <see cref="HeadingColor"/>
        /// </summary>
        protected override void AppendHeading(string heading)
        {
            _console.ForegroundColor = HeadingColor;
            base.AppendHeading(heading);
            _console.ResetColor();
        }
    }
}
