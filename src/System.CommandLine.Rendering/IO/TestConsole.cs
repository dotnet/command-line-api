// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace System.CommandLine.IO
{
    /// <summary>
    /// Provides access to in-memory standard streams that are not attached to <see cref="System.Console"/>.
    /// </summary>
    public class TestConsole : IConsole
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TestConsole"/>.
        /// </summary>
        public TestConsole()
        {
            Out = new StandardStreamWriter();
            Error = new StandardStreamWriter();
        }

        /// <inheritdoc />
        public IStandardStreamWriter Error { get; protected set; }

        /// <inheritdoc />
        public IStandardStreamWriter Out { get; protected set; }

        /// <inheritdoc />
        public bool IsOutputRedirected { get; protected set; }

        /// <inheritdoc />
        public bool IsErrorRedirected { get; protected set; }

        /// <inheritdoc />
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