// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class ForegroundColorSpan : ColorSpan
    {
        public ForegroundColorSpan(string name, AnsiControlCode ansiControlCode)
            : base(name, ansiControlCode)
        {
        }

        public ForegroundColorSpan(RgbColor rgbColor)
            : base(rgbColor,
                   Ansi.Color.Foreground.Rgb(
                       rgbColor.Red,
                       rgbColor.Green,
                       rgbColor.Blue))
        {
        }

        public ForegroundColorSpan(byte r, byte g, byte b)
            : this(new RgbColor(r, g, b))
        {
        }

        public static ForegroundColorSpan Reset() => new(nameof(Reset), Ansi.Color.Foreground.Default);

        public static ForegroundColorSpan Black() => new(nameof(Black), Ansi.Color.Foreground.Black);

        public static ForegroundColorSpan Red() => new(nameof(Red), Ansi.Color.Foreground.Red);

        public static ForegroundColorSpan Green() => new(nameof(Green), Ansi.Color.Foreground.Green);

        public static ForegroundColorSpan Yellow() => new(nameof(Yellow), Ansi.Color.Foreground.Yellow);

        public static ForegroundColorSpan Blue() => new(nameof(Blue), Ansi.Color.Foreground.Blue);

        public static ForegroundColorSpan Magenta() => new(nameof(Magenta), Ansi.Color.Foreground.Magenta);

        public static ForegroundColorSpan Cyan() => new(nameof(Cyan), Ansi.Color.Foreground.Cyan);

        public static ForegroundColorSpan White() => new(nameof(White), Ansi.Color.Foreground.White);

        public static ForegroundColorSpan DarkGray() => new(nameof(DarkGray), Ansi.Color.Foreground.DarkGray);

        public static ForegroundColorSpan LightRed() => new(nameof(LightRed), Ansi.Color.Foreground.LightRed);

        public static ForegroundColorSpan LightGreen() => new(nameof(LightGreen), Ansi.Color.Foreground.LightGreen);

        public static ForegroundColorSpan LightYellow() => new(nameof(LightYellow), Ansi.Color.Foreground.LightYellow);

        public static ForegroundColorSpan LightBlue() => new(nameof(LightBlue), Ansi.Color.Foreground.LightBlue);

        public static ForegroundColorSpan LightMagenta() => new(nameof(LightMagenta), Ansi.Color.Foreground.LightMagenta);

        public static ForegroundColorSpan LightCyan() => new(nameof(LightCyan), Ansi.Color.Foreground.LightCyan);

        public static ForegroundColorSpan LightGray() => new(nameof(LightGray), Ansi.Color.Foreground.LightGray);

        public static ForegroundColorSpan Rgb(byte r, byte g, byte b) => new(r, g, b);
    }
}