// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    internal class NonAnsiRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        private ITerminal Terminal { get; }

        public NonAnsiRenderingSpanVisitor(
            ITerminal terminal,
            Region region) : base(terminal?.Out, region)
        {
            Terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        }

        protected override void SetCursorPosition(
            int? left = null, 
            int? top = null)
        {
            if (left != null && top != null)
            {
                Terminal.SetCursorPosition(left.Value, top.Value);
            }
            else if (top != null)
            {
                Terminal.CursorTop = top.Value;
            }
            else if (left != null)
            {
                Terminal.CursorLeft = left.Value;
            }
        }

        public override void VisitForegroundColorSpan(ForegroundColorSpan span)
        {
            if (span.RgbColor == null &&
                _foregroundColorMappings.TryGetValue(span.Name, out var color))
            {
                Terminal.ForegroundColor = color;
            }
            else
            {
                var backgroundColor = Terminal.BackgroundColor;
                Terminal.ResetColor();
                Terminal.BackgroundColor = backgroundColor;
            }
        }

        public override void VisitBackgroundColorSpan(BackgroundColorSpan span)
        {
            if (span.RgbColor == null &&
                _backgroundColorMappings.TryGetValue(span.Name, out var color))
            {
                Terminal.BackgroundColor = color;
            }
            else
            {
                var foregroundColor = Terminal.ForegroundColor;
                Terminal.ResetColor();
                Terminal.ForegroundColor = foregroundColor;
            }
        }

        public override void VisitCursorControlSpan(CursorControlSpan cursorControlSpan)
        {
            switch(cursorControlSpan.Name)
            {
                case nameof(CursorControlSpan.Hide):
                    Terminal.HideCursor();
                    break;
                case nameof(CursorControlSpan.Show):
                    Terminal.ShowCursor();
                    break;
            }
        }

        private static readonly Dictionary<string, ConsoleColor> _backgroundColorMappings =
            new()
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
            new()
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
