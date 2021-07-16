// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace System.CommandLine.IO
{
    public class TestConsole : IConsole
    {
        public TestConsole()
        {
            Out = new StandardStreamWriter();
            Error = new StandardStreamWriter();
        }

        public IStandardStreamWriter Error { get; protected set; }

        public IStandardStreamWriter Out { get; protected set; }

        public bool IsOutputRedirected { get; protected set; }

        public bool IsErrorRedirected { get; protected set; }

        public bool IsInputRedirected { get; protected set; }

        internal class StandardStreamWriter : TextWriter, IStandardStreamWriter
        {
            private readonly StringBuilder _stringBuilder = new();

            public override void Write(char value)
            {
                _stringBuilder.Append(value);
            }

            public override void Write(string? value)
            {
                _stringBuilder.Append(value);
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public override string ToString() => _stringBuilder.ToString();
        }
    }
}
