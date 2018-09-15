// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine
{
    public interface IConsole
    {
        TextWriter Out { get; }

        TextWriter Error { get; }

        Region GetRegion();

        ConsoleColor ForegroundColor { get; set; }

        void ResetColor();

        int CursorLeft { get; set; }

        int CursorTop { get; set; }

        void SetCursorPosition(int left, int top);

        bool IsOutputRedirected { get; }

        bool IsErrorRedirected { get; }

        bool IsInputRedirected { get; }
    }
}
