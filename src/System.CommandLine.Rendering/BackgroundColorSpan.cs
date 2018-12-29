// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class BackgroundColorSpan : ColorSpan
    {
        public BackgroundColorSpan(string name) : base(name)
        {
        }

        public BackgroundColorSpan(RgbColor rgbColor) : base(rgbColor)
        {
        }

        public BackgroundColorSpan(byte r, byte g, byte b) : this(new RgbColor(r, g, b))
        {
        }

        public static BackgroundColorSpan Reset() => new BackgroundColorSpan(nameof(Reset));

        public static BackgroundColorSpan Black() => new BackgroundColorSpan(nameof(Black));

        public static BackgroundColorSpan Red() => new BackgroundColorSpan(nameof(Red));

        public static BackgroundColorSpan Green() => new BackgroundColorSpan(nameof(Green));

        public static BackgroundColorSpan Yellow() => new BackgroundColorSpan(nameof(Yellow));

        public static BackgroundColorSpan Blue() => new BackgroundColorSpan(nameof(Blue));

        public static BackgroundColorSpan Magenta() => new BackgroundColorSpan(nameof(Magenta));

        public static BackgroundColorSpan Cyan() => new BackgroundColorSpan(nameof(Cyan));

        public static BackgroundColorSpan White() => new BackgroundColorSpan(nameof(White));

        public static BackgroundColorSpan DarkGray() => new BackgroundColorSpan(nameof(DarkGray));

        public static BackgroundColorSpan LightRed() => new BackgroundColorSpan(nameof(LightRed));

        public static BackgroundColorSpan LightGreen() => new BackgroundColorSpan(nameof(LightGreen));

        public static BackgroundColorSpan LightYellow() => new BackgroundColorSpan(nameof(LightYellow));

        public static BackgroundColorSpan LightBlue() => new BackgroundColorSpan(nameof(LightBlue));

        public static BackgroundColorSpan LightMagenta() => new BackgroundColorSpan(nameof(LightMagenta));

        public static BackgroundColorSpan LightCyan() => new BackgroundColorSpan(nameof(LightCyan));

        public static BackgroundColorSpan LightGray() => new BackgroundColorSpan(nameof(LightGray));

        public static BackgroundColorSpan Rgb(byte r, byte g, byte b) => new BackgroundColorSpan(r, g, b);
    }
}
