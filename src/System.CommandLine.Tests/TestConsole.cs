// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Tests
{
    public class TestConsole : IConsole
    {
        public TestConsole()
        {
            Error = new StringWriter();
            Out = new StringWriter();
        }

        public TextWriter Error { get; }

        public TextWriter Out { get; }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int Height { get; set; } = 100;

        public int Width { get; set; } = 100;

        public virtual void ResetColor()
        {
        }

        public Region GetRegion() =>
            new Region(Height,
                         Width,
                         0,
                         0);
    }
}
