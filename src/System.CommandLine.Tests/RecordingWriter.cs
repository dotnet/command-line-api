using System;
using System.IO;
using System.Text;
using Xunit;

namespace System.CommandLine.Tests
{
    public class RecordingWriter : TextWriter
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

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