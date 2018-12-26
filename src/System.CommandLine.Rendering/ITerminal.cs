// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public interface ITerminal :
        CommandLine.ITerminal,
        IDisposable
    {

        Region GetRegion();

        void Clear();

        int CursorLeft { get; set; }

        int CursorTop { get; set; }

        void SetCursorPosition(int left, int top);
    }
}
