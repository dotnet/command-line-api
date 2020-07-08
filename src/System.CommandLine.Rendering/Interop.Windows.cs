// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace System.CommandLine.Rendering
{
    internal static class Interop
    {
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        public const int STD_OUTPUT_HANDLE = -11;

        public const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll")]
        public static extern bool GetConsoleMode(IntPtr handle, out uint mode);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr handle, uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);
    }
}
