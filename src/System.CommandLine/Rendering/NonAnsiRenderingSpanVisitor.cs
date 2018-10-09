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
                _foregroundColorMappings.TryGetValue(span.Name, out var color))
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
                _backgroundColorMappings.TryGetValue(span.Name, out var color))
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

        private static readonly Dictionary<string, ConsoleColor> _backgroundColorMappings =
            new Dictionary<string, ConsoleColor>
            {
                [nameof(BackgroundColorSpan.Black)] = ConsoleColor.Black,
                [nameof(BackgroundColorSpan.Red)] = ConsoleColor.DarkRed,
                [nameof(BackgroundColorSpan.Green)] = ConsoleColor.DarkGreen,
                [nameof(BackgroundColorSpan.Yellow)] = ConsoleColor.DarkYellow,
                [nameof(BackgroundColorSpan.Blue)] = ConsoleColor.DarkBlue,
                [nameof(BackgroundColorSpan.Magenta)] = ConsoleColor.DarkMagenta,
                [nameof(BackgroundColorSpan.Cyan)] = ConsoleColor.DarkCyan,
                [nameof(BackgroundColorSpan.White)] = ConsoleColor.White,
                [nameof(BackgroundColorSpan.DarkGray)] = ConsoleColor.DarkGray,
                [nameof(BackgroundColorSpan.LightRed)] = ConsoleColor.Red,
                [nameof(BackgroundColorSpan.LightGreen)] = ConsoleColor.Green,
                [nameof(BackgroundColorSpan.LightYellow)] = ConsoleColor.Yellow,
                [nameof(BackgroundColorSpan.LightBlue)] = ConsoleColor.Blue,
                [nameof(BackgroundColorSpan.LightMagenta)] = ConsoleColor.Magenta,
                [nameof(BackgroundColorSpan.LightCyan)] = ConsoleColor.Cyan,
                [nameof(BackgroundColorSpan.LightGray)] = ConsoleColor.Gray,
            };

        private static readonly Dictionary<string, ConsoleColor> _foregroundColorMappings =
            new Dictionary<string, ConsoleColor>
            {
                [nameof(ForegroundColorSpan.Black)] = ConsoleColor.Black,
                [nameof(ForegroundColorSpan.Red)] = ConsoleColor.DarkRed,
                [nameof(ForegroundColorSpan.Green)] = ConsoleColor.DarkGreen,
                [nameof(ForegroundColorSpan.Yellow)] = ConsoleColor.DarkYellow,
                [nameof(ForegroundColorSpan.Blue)] = ConsoleColor.DarkBlue,
                [nameof(ForegroundColorSpan.Magenta)] = ConsoleColor.DarkMagenta,
                [nameof(ForegroundColorSpan.Cyan)] = ConsoleColor.DarkCyan,
                [nameof(ForegroundColorSpan.White)] = ConsoleColor.White,
                [nameof(ForegroundColorSpan.DarkGray)] = ConsoleColor.DarkGray,
                [nameof(ForegroundColorSpan.LightRed)] = ConsoleColor.Red,
                [nameof(ForegroundColorSpan.LightGreen)] = ConsoleColor.Green,
                [nameof(ForegroundColorSpan.LightYellow)] = ConsoleColor.Yellow,
                [nameof(ForegroundColorSpan.LightBlue)] = ConsoleColor.Blue,
                [nameof(ForegroundColorSpan.LightMagenta)] = ConsoleColor.Magenta,
                [nameof(ForegroundColorSpan.LightCyan)] = ConsoleColor.Cyan,
                [nameof(ForegroundColorSpan.LightGray)] = ConsoleColor.Gray,
            };
    }
}
