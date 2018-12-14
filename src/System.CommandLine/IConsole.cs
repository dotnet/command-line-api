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

        // FIX: (SetOut) remove, use System.Console
        void SetOut(TextWriter writer);

        ConsoleColor BackgroundColor { get; set; }

        ConsoleColor ForegroundColor { get; set; }

        void ResetColor();

        int CursorLeft { get; set; }

        int CursorTop { get; set; }

        void SetCursorPosition(int left, int top);

        // FIX extract interface / separate implementation in System.CommandLine.Rendering
        bool IsVirtualTerminal { get; }

        void TryEnableVirtualTerminal();
    }

    public interface IStandardOut 
    {
        IStandardStreamWriter Out { get; }

        // FIX extract interface / separate implementation
        bool IsOutputRedirected { get; }
    }

    public interface IStandardError 
    {
        IStandardStreamWriter Error { get; }
    }

    public interface IStandardStream
    {
    }

    public interface IStandardIn : IStandardStream
    {
    }

    public interface IStandardStreamWriter : IStandardStream
    {
        void Write(string value);
    }

    public interface IStandardStreamReader : IStandardStream
    {
    }

    public interface IConsole : IStandardOut, IStandardError, IStandardIn
    {
    }
}
