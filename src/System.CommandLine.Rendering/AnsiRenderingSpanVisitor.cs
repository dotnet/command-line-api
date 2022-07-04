// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using static System.CommandLine.Rendering.Ansi;

namespace System.CommandLine.Rendering
{
    internal class AnsiRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public AnsiRenderingSpanVisitor(
            IConsole console,
            Region region) : base(console.Out, region)
        {
        }

        protected override void SetCursorPosition(int? left = null, int? top = null)
        {
            if (Region == Region.Scrolling)
            {
                Writer.WriteLine(
                    Cursor.Move
                          .ToLocation(left: left + 1)
                          .EscapeSequence);
            }
            else
            {
                Writer.Write(
                    Cursor.Move
                          .ToLocation(left: left + 1, top: top + 1)
                          .EscapeSequence);
            }
        }

        public override void VisitForegroundColorSpan(ForegroundColorSpan span)
        {
            AnsiControlCode controlCode;

            if (span.RgbColor is RgbColor rgb)
            {
                controlCode = Color.Foreground.Rgb(rgb.Red, rgb.Green, rgb.Blue);
            }
            else if (!_foregroundColorControlCodeMappings.TryGetValue(span.Name, out controlCode))
            {
                return;
            }

            Writer.Write(controlCode.EscapeSequence);
        }

        public override void VisitBackgroundColorSpan(BackgroundColorSpan span)
        {
            AnsiControlCode controlCode;

            if (span.RgbColor is RgbColor rgb)
            {
                controlCode = Color.Background.Rgb(rgb.Red, rgb.Green, rgb.Blue);
            }
            else if (!_backgroundColorControlCodeMappings.TryGetValue(span.Name, out controlCode))
            {
                return;
            }

            Writer.Write(controlCode.EscapeSequence);
        }

        public override void VisitStyleSpan(StyleSpan span)
        {
            if (_styleControlCodeMappings.TryGetValue(span.Name, out var controlCode))
            {
                Writer.Write(controlCode.EscapeSequence);
            }
        }

        public override void VisitCursorControlSpan(CursorControlSpan cursorControlSpan)
        {
            if (_styleControlCodeMappings.TryGetValue(cursorControlSpan.Name, out var controlCode))
            {
                Writer.Write(controlCode.EscapeSequence);
            }
        }

        private static readonly Dictionary<string, AnsiControlCode> _foregroundColorControlCodeMappings =
            new()
            {
                [nameof(ForegroundColorSpan.Reset)] = Color.Foreground.Default,
                [nameof(ForegroundColorSpan.Black)] = Color.Foreground.Black,
                [nameof(ForegroundColorSpan.Red)] = Color.Foreground.Red,
                [nameof(ForegroundColorSpan.Green)] = Color.Foreground.Green,
                [nameof(ForegroundColorSpan.Yellow)] = Color.Foreground.Yellow,
                [nameof(ForegroundColorSpan.Blue)] = Color.Foreground.Blue,
                [nameof(ForegroundColorSpan.Magenta)] = Color.Foreground.Magenta,
                [nameof(ForegroundColorSpan.Cyan)] = Color.Foreground.Cyan,
                [nameof(ForegroundColorSpan.White)] = Color.Foreground.White,
                [nameof(ForegroundColorSpan.DarkGray)] = Color.Foreground.DarkGray,
                [nameof(ForegroundColorSpan.LightRed)] = Color.Foreground.LightRed,
                [nameof(ForegroundColorSpan.LightGreen)] = Color.Foreground.LightGreen,
                [nameof(ForegroundColorSpan.LightYellow)] = Color.Foreground.LightYellow,
                [nameof(ForegroundColorSpan.LightBlue)] = Color.Foreground.LightBlue,
                [nameof(ForegroundColorSpan.LightMagenta)] = Color.Foreground.LightMagenta,
                [nameof(ForegroundColorSpan.LightCyan)] = Color.Foreground.LightCyan,
                [nameof(ForegroundColorSpan.LightGray)] = Color.Foreground.LightGray,
            };

        private static readonly Dictionary<string, AnsiControlCode> _backgroundColorControlCodeMappings =
            new()
            {
                [nameof(BackgroundColorSpan.Reset)] = Color.Background.Default,
                [nameof(BackgroundColorSpan.Black)] = Color.Background.Black,
                [nameof(BackgroundColorSpan.Red)] = Color.Background.Red,
                [nameof(BackgroundColorSpan.Green)] = Color.Background.Green,
                [nameof(BackgroundColorSpan.Yellow)] = Color.Background.Yellow,
                [nameof(BackgroundColorSpan.Blue)] = Color.Background.Blue,
                [nameof(BackgroundColorSpan.Magenta)] = Color.Background.Magenta,
                [nameof(BackgroundColorSpan.Cyan)] = Color.Background.Cyan,
                [nameof(BackgroundColorSpan.White)] = Color.Background.White,
                [nameof(BackgroundColorSpan.DarkGray)] = Color.Background.DarkGray,
                [nameof(BackgroundColorSpan.LightRed)] = Color.Background.LightRed,
                [nameof(BackgroundColorSpan.LightGreen)] = Color.Background.LightGreen,
                [nameof(BackgroundColorSpan.LightYellow)] = Color.Background.LightYellow,
                [nameof(BackgroundColorSpan.LightBlue)] = Color.Background.LightBlue,
                [nameof(BackgroundColorSpan.LightMagenta)] = Color.Background.LightMagenta,
                [nameof(BackgroundColorSpan.LightCyan)] = Color.Background.LightCyan,
                [nameof(BackgroundColorSpan.LightGray)] = Color.Background.LightGray,
            };

        private static readonly Dictionary<string, AnsiControlCode> _styleControlCodeMappings =
            new()
            {
                [nameof(StyleSpan.AttributesOff)] = Ansi.Text.AttributesOff,
                [nameof(StyleSpan.BlinkOff)] = Ansi.Text.BlinkOff,
                [nameof(StyleSpan.BlinkOn)] = Ansi.Text.BlinkOn,
                [nameof(StyleSpan.BoldOff)] = Ansi.Text.BoldOff,
                [nameof(StyleSpan.BoldOn)] = Ansi.Text.BoldOn,
                [nameof(StyleSpan.HiddenOn)] = Ansi.Text.HiddenOn,
                [nameof(StyleSpan.ReverseOn)] = Ansi.Text.ReverseOn,
                [nameof(StyleSpan.ReverseOff)] = Ansi.Text.ReverseOff,
                [nameof(StyleSpan.StandoutOff)] = Ansi.Text.StandoutOff,
                [nameof(StyleSpan.StandoutOn)] = Ansi.Text.StandoutOn,
                [nameof(StyleSpan.UnderlinedOff)] = Ansi.Text.UnderlinedOff,
                [nameof(StyleSpan.UnderlinedOn)] = Ansi.Text.UnderlinedOn,
            };
    }
}