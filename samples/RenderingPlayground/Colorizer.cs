// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;

namespace RenderingPlayground
{
    internal static class Colorizer
    {
        public static TextSpan Underline(this string value) =>
            new ContainerSpan(StyleSpan.UnderlinedOn(),
                              new ContentSpan(value),
                              StyleSpan.UnderlinedOff());  
                              
                              
        public static TextSpan Rgb(this string value, byte r, byte g, byte b) =>
            new ContainerSpan(ForegroundColorSpan.Rgb(r, g, b),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());

        public static TextSpan LightGreen(this string value) =>
            new ContainerSpan(ForegroundColorSpan.LightGreen(),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());

        public static TextSpan White(this string value) =>
            new ContainerSpan(ForegroundColorSpan.White(),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());
    }
}
