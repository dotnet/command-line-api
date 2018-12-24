// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    internal class EntireConsoleRegion : Region
    {
        public static EntireConsoleRegion Instance { get; } = new EntireConsoleRegion();

        private EntireConsoleRegion() : base(0, 0, Console.WindowWidth, Console.WindowHeight, false)
        {
        }

        public override int Height => Console.WindowHeight;

        public override int Width => Console.WindowWidth;

        public override int Top => Console.CursorTop;

        public override int Left => Console.CursorLeft;
    }
}
