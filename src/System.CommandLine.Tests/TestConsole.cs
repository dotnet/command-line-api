// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        public void SetOut(TextWriter newOut)
        {
        }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int WindowWidth { get; set; }

        public virtual void ResetColor()
        {
        }
    }
}
