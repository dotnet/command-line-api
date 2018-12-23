// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal class SystemTerminal :
        CommandLine.SystemTerminal,
        ITerminal
    {
        private VirtualTerminalMode _virtualTerminalMode;

        public void SetOut(TextWriter writer) => Console.SetOut(writer);

        public Region GetRegion() =>
            IsOutputRedirected
                ? new Region(0, 0, int.MaxValue, int.MaxValue, false)
                : EntireConsoleRegion.Instance;

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public bool IsVirtualTerminal
        {
            get
            {
                if (_virtualTerminalMode != null)
                {
                    return _virtualTerminalMode.IsEnabled;
                }

                var terminalName = Environment.GetEnvironmentVariable("TERM");

                return !string.IsNullOrEmpty(terminalName)
                       && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase);
            }
        }

        public void TryEnableVirtualTerminal()
        {
            if (IsOutputRedirected)
            {
                return;
            }

            _virtualTerminalMode = VirtualTerminalMode.TryEnable();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _virtualTerminalMode?.Dispose();
            }
        }

        internal class RecordingWriter : TextWriter, IStandardStreamWriter
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
}
