namespace System.CommandLine
{
    public class ColorHelpBuilder : HelpBuilder
    {
        public ColorHelpBuilder(
            IConsole console,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
        }

        protected override void AddHeading(string heading)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            base.AddHeading(heading);
            Console.ResetColor();
        }
    }
}
