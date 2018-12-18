// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine
{
    public interface ITerminal :
        IConsole,
        IDisposable
    {
        Region GetRegion();

        void SetOut(TextWriter writer);

        ConsoleColor BackgroundColor { get; set; }

        ConsoleColor ForegroundColor { get; set; }

        void ResetColor();

        int CursorLeft { get; set; }

        int CursorTop { get; set; }

        void SetCursorPosition(int left, int top);
      
        bool IsVirtualTerminal { get; }

        void TryEnableVirtualTerminal();
    }
}
