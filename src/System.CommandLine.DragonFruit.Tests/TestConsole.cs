// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace System.CommandLine.DragonFruit.Tests
{
    internal class TestConsole : IConsole
    {
        public TestConsole(ITestOutputHelper output)
        {
            var writer = new TestOutputWriter(output);
            Error = writer;
            Out = writer;
        }

        public TextWriter Error { get; set; }
        public TextWriter Out { get; set; }
        public ConsoleColor ForegroundColor { get; set; }

        public void ResetColor() { }

        private class TestOutputWriter : TextWriter
        {
            private readonly ITestOutputHelper _output;
            private readonly StringBuilder _sb = new StringBuilder();

            public TestOutputWriter(ITestOutputHelper output)
            {
                _output = output;
            }

            public override Encoding Encoding => Encoding.Unicode;

            public override void Write(char value)
            {
                if (value == '\r' || value == '\n')
                {
                    if (_sb.Length > 0)
                    {
                        _output.WriteLine(_sb.ToString());
                        _sb.Clear();
                    }
                }
                else
                {
                    _sb.Append(value);
                }
            }
        }
    }
}
