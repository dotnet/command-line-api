using System.Collections.Generic;
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

        protected override void SetCursorPosition(int left, int top)
        {
            Writer.Write(
                Cursor.Move
                      .ToLocation(left: left + 1, top: top + 1)
                      .EscapeSequence);
        }

        public override void VisitForegroundColorSpan(ForegroundColorSpan span)
        {
            AnsiControlCode controlCode;

            if (span.RgbColor is RgbColor rgb)
            {
                controlCode = Color.Foreground.Rgb(rgb.Red, rgb.Green, rgb.Blue);
            }
            else if (!_foregroundColorControlCodeMappings.TryGetValue(span, out controlCode))
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
            else if (!_backgroundColorControlCodeMappings.TryGetValue(span, out controlCode))
            {
                return;
            }

            Writer.Write(controlCode.EscapeSequence);
        }

        public override void VisitStyleSpan(StyleSpan span)
        {
            if (_styleControlCodeMappings.TryGetValue(span, out var controlCode))
            {
                Writer.Write(controlCode.EscapeSequence);
            }
        }

        private static readonly Dictionary<ForegroundColorSpan, AnsiControlCode> _foregroundColorControlCodeMappings =
            new Dictionary<ForegroundColorSpan, AnsiControlCode>
            {
                [ForegroundColorSpan.Reset] = Color.Foreground.Default,
                [ForegroundColorSpan.Black] = Color.Foreground.Black,
                [ForegroundColorSpan.Red] = Color.Foreground.Red,
                [ForegroundColorSpan.Green] = Color.Foreground.Green,
                [ForegroundColorSpan.Yellow] = Color.Foreground.Yellow,
                [ForegroundColorSpan.Blue] = Color.Foreground.Blue,
                [ForegroundColorSpan.Magenta] = Color.Foreground.Magenta,
                [ForegroundColorSpan.Cyan] = Color.Foreground.Cyan,
                [ForegroundColorSpan.White] = Color.Foreground.White,
                [ForegroundColorSpan.DarkGray] = Color.Foreground.DarkGray,
                [ForegroundColorSpan.LightRed] = Color.Foreground.LightRed,
                [ForegroundColorSpan.LightGreen] = Color.Foreground.LightGreen,
                [ForegroundColorSpan.LightYellow] = Color.Foreground.LightYellow,
                [ForegroundColorSpan.LightBlue] = Color.Foreground.LightBlue,
                [ForegroundColorSpan.LightMagenta] = Color.Foreground.LightMagenta,
                [ForegroundColorSpan.LightCyan] = Color.Foreground.LightCyan,
                [ForegroundColorSpan.LightGray] = Color.Foreground.LightGray,
            };

        private static readonly Dictionary<BackgroundColorSpan, AnsiControlCode> _backgroundColorControlCodeMappings =
            new Dictionary<BackgroundColorSpan, AnsiControlCode>
            {
                [BackgroundColorSpan.Reset] = Color.Background.Default,
                [BackgroundColorSpan.Black] = Color.Background.Black,
                [BackgroundColorSpan.Red] = Color.Background.Red,
                [BackgroundColorSpan.Green] = Color.Background.Green,
                [BackgroundColorSpan.Yellow] = Color.Background.Yellow,
                [BackgroundColorSpan.Blue] = Color.Background.Blue,
                [BackgroundColorSpan.Magenta] = Color.Background.Magenta,
                [BackgroundColorSpan.Cyan] = Color.Background.Cyan,
                [BackgroundColorSpan.White] = Color.Background.White,
                [BackgroundColorSpan.DarkGray] = Color.Background.DarkGray,
                [BackgroundColorSpan.LightRed] = Color.Background.LightRed,
                [BackgroundColorSpan.LightGreen] = Color.Background.LightGreen,
                [BackgroundColorSpan.LightYellow] = Color.Background.LightYellow,
                [BackgroundColorSpan.LightBlue] = Color.Background.LightBlue,
                [BackgroundColorSpan.LightMagenta] = Color.Background.LightMagenta,
                [BackgroundColorSpan.LightCyan] = Color.Background.LightCyan,
                [BackgroundColorSpan.LightGray] = Color.Background.LightGray,
            };

        private static readonly Dictionary<StyleSpan, AnsiControlCode> _styleControlCodeMappings =
            new Dictionary<StyleSpan, AnsiControlCode>
            {
                [StyleSpan.BlinkOff] = Ansi.Text.BlinkOff,
                [StyleSpan.BlinkOn] = Ansi.Text.BlinkOn,
                [StyleSpan.BoldOff] = Ansi.Text.BoldOff,
                [StyleSpan.BoldOn] = Ansi.Text.BoldOn,
                [StyleSpan.HiddenOn] = Ansi.Text.HiddenOn,
                [StyleSpan.ReverseOn] = Ansi.Text.ReverseOn,
                [StyleSpan.ReversOff] = Ansi.Text.ReversOff,
                [StyleSpan.StandoutOff] = Ansi.Text.StandoutOff,
                [StyleSpan.StandoutOn] = Ansi.Text.StandoutOn,
                [StyleSpan.UnderlinedOff] = Ansi.Text.UnderlinedOff,
                [StyleSpan.UnderlinedOn] = Ansi.Text.UnderlinedOn,
            };
    }
}
