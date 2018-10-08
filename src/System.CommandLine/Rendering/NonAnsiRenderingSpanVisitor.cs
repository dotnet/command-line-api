using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    internal class NonAnsiRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public IConsole Console { get; }

        public NonAnsiRenderingSpanVisitor(
            IConsole console,
            Region region) : base(console?.Out, region)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        protected override void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        public override void VisitForegroundColorSpan(ForegroundColorSpan span)
        {
            if (span.RgbColor == null &&
                _foregroundColorMappings.TryGetValue(span, out var color))
            {
                Console.ForegroundColor = color;
            }
            else
            {
                var backgroundColor = Console.BackgroundColor;
                Console.ResetColor();
                Console.BackgroundColor = backgroundColor;
            }
        }

        public override void VisitBackgroundColorSpan(BackgroundColorSpan span)
        {
            if (span.RgbColor == null &&
                _backgroundColorMappings.TryGetValue(span, out var color))
            {
                Console.BackgroundColor = color;
            }
            else
            {
                var foregroundColor = Console.ForegroundColor;
                Console.ResetColor();
                Console.ForegroundColor = foregroundColor;
            }
        }

        private static readonly Dictionary<BackgroundColorSpan, ConsoleColor> _backgroundColorMappings =
            new Dictionary<BackgroundColorSpan, ConsoleColor>
            {
                [BackgroundColorSpan.Black] = ConsoleColor.Black,
                [BackgroundColorSpan.Red] = ConsoleColor.DarkRed,
                [BackgroundColorSpan.Green] = ConsoleColor.DarkGreen,
                [BackgroundColorSpan.Yellow] = ConsoleColor.DarkYellow,
                [BackgroundColorSpan.Blue] = ConsoleColor.DarkBlue,
                [BackgroundColorSpan.Magenta] = ConsoleColor.DarkMagenta,
                [BackgroundColorSpan.Cyan] = ConsoleColor.DarkCyan,
                [BackgroundColorSpan.White] = ConsoleColor.White,
                [BackgroundColorSpan.DarkGray] = ConsoleColor.DarkGray,
                [BackgroundColorSpan.LightRed] = ConsoleColor.Red,
                [BackgroundColorSpan.LightGreen] = ConsoleColor.Green,
                [BackgroundColorSpan.LightYellow] = ConsoleColor.Yellow,
                [BackgroundColorSpan.LightBlue] = ConsoleColor.Blue,
                [BackgroundColorSpan.LightMagenta] = ConsoleColor.Magenta,
                [BackgroundColorSpan.LightCyan] = ConsoleColor.Cyan,
                [BackgroundColorSpan.LightGray] = ConsoleColor.Gray,
            };

        private static readonly Dictionary<ForegroundColorSpan, ConsoleColor> _foregroundColorMappings =
            new Dictionary<ForegroundColorSpan, ConsoleColor>
            {
                [ForegroundColorSpan.Black] = ConsoleColor.Black,
                [ForegroundColorSpan.Red] = ConsoleColor.DarkRed,
                [ForegroundColorSpan.Green] = ConsoleColor.DarkGreen,
                [ForegroundColorSpan.Yellow] = ConsoleColor.DarkYellow,
                [ForegroundColorSpan.Blue] = ConsoleColor.DarkBlue,
                [ForegroundColorSpan.Magenta] = ConsoleColor.DarkMagenta,
                [ForegroundColorSpan.Cyan] = ConsoleColor.DarkCyan,
                [ForegroundColorSpan.White] = ConsoleColor.White,
                [ForegroundColorSpan.DarkGray] = ConsoleColor.DarkGray,
                [ForegroundColorSpan.LightRed] = ConsoleColor.Red,
                [ForegroundColorSpan.LightGreen] = ConsoleColor.Green,
                [ForegroundColorSpan.LightYellow] = ConsoleColor.Yellow,
                [ForegroundColorSpan.LightBlue] = ConsoleColor.Blue,
                [ForegroundColorSpan.LightMagenta] = ConsoleColor.Magenta,
                [ForegroundColorSpan.LightCyan] = ConsoleColor.Cyan,
                [ForegroundColorSpan.LightGray] = ConsoleColor.Gray,
            };
    }
}
