// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.IO;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal class RecordingWriter : TextWriter, IStandardStreamWriter
    {
        private readonly StringBuilder _stringBuilder = new();

        public event Action<char> CharWritten;

        public override void Write(char value)
        {
            _stringBuilder.Append(value);
            CharWritten?.Invoke(value);
        }

        public override Encoding Encoding { get; } = Encoding.Unicode;

        public override string ToString() => _stringBuilder.ToString();
    }
}
