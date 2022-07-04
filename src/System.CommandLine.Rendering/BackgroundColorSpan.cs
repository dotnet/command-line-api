// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class BackgroundColorSpan : ColorSpan
    {
        public BackgroundColorSpan(string name, AnsiControlCode ansiControlCode)
            : base(name, ansiControlCode)
        {
        }

        public BackgroundColorSpan(RgbColor rgbColor)
            : base(rgbColor,
                   Ansi.Color.Foreground.Rgb(
                       rgbColor.Red,
                       rgbColor.Green,
                       rgbColor.Blue))
        {
        }

        public BackgroundColorSpan(byte r, byte g, byte b)
            : this(new RgbColor(r, g, b))
        {
        }

        public static BackgroundColorSpan Reset() => new(nameof(Reset), Ansi.Color.Background.Default);

        public static BackgroundColorSpan Black() => new(nameof(Black), Ansi.Color.Background.Black);

        public static BackgroundColorSpan Red() => new(nameof(Red), Ansi.Color.Background.Red);

        public static BackgroundColorSpan Green() => new(nameof(Green), Ansi.Color.Background.Green);

        public static BackgroundColorSpan Yellow() => new(nameof(Yellow), Ansi.Color.Background.Yellow);

        public static BackgroundColorSpan Blue() => new(nameof(Blue), Ansi.Color.Background.Blue);

        public static BackgroundColorSpan Magenta() => new(nameof(Magenta), Ansi.Color.Background.Magenta);

        public static BackgroundColorSpan Cyan() => new(nameof(Cyan), Ansi.Color.Background.Cyan);

        public static BackgroundColorSpan White() => new(nameof(White), Ansi.Color.Background.White);

        public static BackgroundColorSpan DarkGray() => new(nameof(DarkGray), Ansi.Color.Background.DarkGray);

        public static BackgroundColorSpan LightRed() => new(nameof(LightRed), Ansi.Color.Background.LightRed);

        public static BackgroundColorSpan LightGreen() => new(nameof(LightGreen), Ansi.Color.Background.LightGreen);

        public static BackgroundColorSpan LightYellow() => new(nameof(LightYellow), Ansi.Color.Background.LightYellow);

        public static BackgroundColorSpan LightBlue() => new(nameof(LightBlue), Ansi.Color.Background.LightBlue);

        public static BackgroundColorSpan LightMagenta() => new(nameof(LightMagenta), Ansi.Color.Background.LightMagenta);

        public static BackgroundColorSpan LightCyan() => new(nameof(LightCyan), Ansi.Color.Background.LightCyan);

        public static BackgroundColorSpan LightGray() => new(nameof(LightGray), Ansi.Color.Background.LightGray);

        public static BackgroundColorSpan Rgb(byte r, byte g, byte b) => new(r, g, b);
    }
}
