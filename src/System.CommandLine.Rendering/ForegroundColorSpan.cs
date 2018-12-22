// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class ForegroundColorSpan : ColorSpan
    {
        public ForegroundColorSpan(string name) : base(name)
        {
        }

        public ForegroundColorSpan(RgbColor rgbColor) : base(rgbColor)
        {
        }

        public ForegroundColorSpan(byte r, byte g, byte b) : this(new RgbColor(r, g, b))
        {
        }

        public static ForegroundColorSpan Reset() => new ForegroundColorSpan(nameof(Reset));

        public static ForegroundColorSpan Black() => new ForegroundColorSpan(nameof(Black));

        public static ForegroundColorSpan Red() => new ForegroundColorSpan(nameof(Red));

        public static ForegroundColorSpan Green() => new ForegroundColorSpan(nameof(Green));

        public static ForegroundColorSpan Yellow() => new ForegroundColorSpan(nameof(Yellow));

        public static ForegroundColorSpan Blue() => new ForegroundColorSpan(nameof(Blue));

        public static ForegroundColorSpan Magenta() => new ForegroundColorSpan(nameof(Magenta));

        public static ForegroundColorSpan Cyan() => new ForegroundColorSpan(nameof(Cyan));

        public static ForegroundColorSpan White() => new ForegroundColorSpan(nameof(White));

        public static ForegroundColorSpan DarkGray() => new ForegroundColorSpan(nameof(DarkGray));

        public static ForegroundColorSpan LightRed() => new ForegroundColorSpan(nameof(LightRed));

        public static ForegroundColorSpan LightGreen() => new ForegroundColorSpan(nameof(LightGreen));

        public static ForegroundColorSpan LightYellow() => new ForegroundColorSpan(nameof(LightYellow));

        public static ForegroundColorSpan LightBlue() => new ForegroundColorSpan(nameof(LightBlue));

        public static ForegroundColorSpan LightMagenta() => new ForegroundColorSpan(nameof(LightMagenta));

        public static ForegroundColorSpan LightCyan() => new ForegroundColorSpan(nameof(LightCyan));

        public static ForegroundColorSpan LightGray() => new ForegroundColorSpan(nameof(LightGray));

        public static ForegroundColorSpan Rgb(byte r, byte g, byte b) => new ForegroundColorSpan(r, g, b);
    }
}
