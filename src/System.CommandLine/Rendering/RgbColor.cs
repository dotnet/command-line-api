// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class RgbColor
    {
        public RgbColor(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
    }
}
