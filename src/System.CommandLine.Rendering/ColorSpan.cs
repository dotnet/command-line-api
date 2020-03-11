// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public abstract class ColorSpan : ControlSpan
    {
        protected ColorSpan(string name, AnsiControlCode ansiControlCode)
            : base(name, ansiControlCode)
        {
        }

        protected ColorSpan(RgbColor rgbColor, AnsiControlCode ansiControlCode)
            : base(
                GetName(rgbColor) ?? throw new ArgumentNullException(nameof(rgbColor)), 
                ansiControlCode)
        {
            RgbColor = rgbColor;
        }

        public RgbColor RgbColor { get; }

        protected static string GetName(RgbColor rgbColor)
        {
            return rgbColor?.ToString();
        }
    }
}