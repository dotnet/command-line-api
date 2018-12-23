// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Rendering
{
    public interface ITerminal :
        System.CommandLine.ITerminal,
        IDisposable
    {
        Region GetRegion();

        void SetOut(TextWriter writer);

        int CursorLeft { get; set; }

        int CursorTop { get; set; }

        void SetCursorPosition(int left, int top);
      
        bool IsVirtualTerminal { get; }

        void TryEnableVirtualTerminal();
    }
}
