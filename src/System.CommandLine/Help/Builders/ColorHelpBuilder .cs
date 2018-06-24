namespace System.CommandLine
{
    public class ColorHelpBuilder : HelpBuilder
    {
        protected const ConsoleColor DefaultHeadingColor = ConsoleColor.Cyan;

        public ConsoleColor HeadingColor { get; } = DefaultHeadingColor;

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

        protected override void AppendHeading(string heading)
        {
            _console.ForegroundColor = HeadingColor;
            base.AppendHeading(heading);
            _console.ResetColor();
        }
    }
}
