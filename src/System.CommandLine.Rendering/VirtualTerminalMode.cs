// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using static System.CommandLine.Rendering.Interop;

namespace System.CommandLine.Rendering
{
    public sealed class VirtualTerminalMode : IDisposable
    {
        private readonly IntPtr _stdOutHandle;
        private readonly IntPtr _stdInHandle;
        private readonly uint _originalOutputMode;
        private readonly uint _originalInputMode;

        private VirtualTerminalMode(
            bool isEnabled,
            uint? error = null)
        {
            IsEnabled = isEnabled;
            Error = error;
        }

        private VirtualTerminalMode(
            IntPtr stdOutHandle,
            uint originalOutputMode,
            IntPtr stdInHandle,
            uint originalInputMode)
        {
            IsEnabled = true;

            _stdOutHandle = stdOutHandle;
            _originalOutputMode = originalOutputMode;
            _stdInHandle = stdInHandle;
            _originalInputMode = originalInputMode;
        }

        public uint? Error { get; }

        public bool IsEnabled { get; }

        public static VirtualTerminalMode TryEnable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                var stdInHandle = GetStdHandle(STD_INPUT_HANDLE);

                if (!GetConsoleMode(stdOutHandle, out var originalOutputMode))
                {
                    return new VirtualTerminalMode(false, GetLastError());
                }

                if (!GetConsoleMode(stdInHandle, out var originalInputMode))
                {
                    return new VirtualTerminalMode(false, GetLastError());
                }

                // var requestedInputMode = originalInputMode |
                //                          ENABLE_VIRTUAL_TERMINAL_INPUT;

                var requestedOutputMode = originalOutputMode |
                                          ENABLE_VIRTUAL_TERMINAL_PROCESSING |
                                          DISABLE_NEWLINE_AUTO_RETURN;

                if (!SetConsoleMode(stdOutHandle, requestedOutputMode))
                {
                    return new VirtualTerminalMode(false, GetLastError());
                }

                // if (!SetConsoleMode(stdInHandle, requestedInputMode))
                // {
                //     return new VirtualTerminalMode(false, GetLastError());
                // }

                return new VirtualTerminalMode(stdOutHandle,
                                               originalOutputMode,
                                               stdInHandle,
                                               originalInputMode);
            }

            var terminalName = Environment.GetEnvironmentVariable("TERM");

            var isXterm = !string.IsNullOrEmpty(terminalName)
                          && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase);

            // TODO: Is this a reasonable default?
            return new VirtualTerminalMode(isXterm);
        }

        private void RestoreConsoleMode()
        {
            if (IsEnabled)
            {
                if (_stdOutHandle != IntPtr.Zero)
                {
                    SetConsoleMode(_stdOutHandle, _originalOutputMode);
                }

                // if (_stdInHandle != IntPtr.Zero)
                // {
                //    SetConsoleMode(_stdInHandle, _originalInputMode);
                // }
            }
        }

        public void Dispose()
        {
            RestoreConsoleMode();
            GC.SuppressFinalize(this);
        }

        ~VirtualTerminalMode()
        {
            RestoreConsoleMode();
        }
    }
}
