// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    internal class ScrollingTerminalRegion : Region
    {
        public ScrollingTerminalRegion() : base(0, 0, isOverwrittenOnRender: false)
        {
        }

        public override int Height => int.MaxValue;

        public override int Width => Console.WindowWidth;

        public override int Top => Console.CursorTop;

        public override int Left => Console.CursorLeft;
    }
}
