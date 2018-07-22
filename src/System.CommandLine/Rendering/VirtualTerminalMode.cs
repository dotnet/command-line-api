using System.Runtime.InteropServices;

namespace System.CommandLine.Rendering
{
    public class VirtualTerminalMode : IDisposable
    {
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        private const int STD_OUTPUT_HANDLE = -11;

        private const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(
            IntPtr handle,
            out uint mode);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(
            IntPtr handle,
            uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int handle);

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
            var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            var stdInHandle = GetStdHandle(STD_INPUT_HANDLE);

            if (!GetConsoleMode(stdOutHandle, out var originalOutputMode))
            {
                return new VirtualTerminalMode(false);
            }

            if (!GetConsoleMode(stdInHandle, out var originalInputMode))
            {
                return new VirtualTerminalMode(false);
            }

            var requestedInputMode = originalInputMode |
                                     ENABLE_VIRTUAL_TERMINAL_INPUT;

            var requestedOutputMode = originalOutputMode |
                                      ENABLE_VIRTUAL_TERMINAL_PROCESSING |
                                      DISABLE_NEWLINE_AUTO_RETURN;

            if (!SetConsoleMode(stdOutHandle, requestedOutputMode))
            {
                return new VirtualTerminalMode(false, GetLastError());
            }

            if (!SetConsoleMode(stdInHandle, requestedInputMode))
            {
                return new VirtualTerminalMode(false, GetLastError());
            }

            return new VirtualTerminalMode(stdOutHandle,
                                           originalOutputMode,
                                           stdInHandle,
                                           originalInputMode);
        }

        public void Dispose()
        {
            SetConsoleMode(_stdOutHandle, _originalOutputMode);
            SetConsoleMode(_stdInHandle, _originalInputMode);
        }
    }
}
