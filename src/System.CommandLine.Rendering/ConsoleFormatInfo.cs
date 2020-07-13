// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace System.CommandLine.Rendering
{
    public sealed class ConsoleFormatInfo : IFormatProvider
    {
        private static ConsoleFormatInfo s_currentInfo;
        private bool _isReadOnly;
        private bool _supportsAnsiCodes;

        public ConsoleFormatInfo()
        {
        }

        public static ConsoleFormatInfo CurrentInfo
        {
            get
            {
                return s_currentInfo ??=
                    InitializeCurrentInfo();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                s_currentInfo = ReadOnly(value);
            }
        }

        public bool SupportsAnsiCodes
        {
            get => _supportsAnsiCodes;
            set
            {
                VerifyWritable();
                _supportsAnsiCodes = value;
            }
        }

        public bool IsReadOnly => _isReadOnly;

        public static ConsoleFormatInfo GetInstance(IFormatProvider formatProvider)
        {
            return formatProvider == null ?
                CurrentInfo : // Fast path for a null provider
                GetProviderNonNull(formatProvider);

            static ConsoleFormatInfo GetProviderNonNull(IFormatProvider provider)
            {
                return
                    provider as ConsoleFormatInfo ?? // Fast path for an CFI
                    provider.GetFormat(typeof(ConsoleFormatInfo)) as ConsoleFormatInfo ??
                    CurrentInfo;
            }
        }

        public object GetFormat(Type formatType) =>
            formatType == typeof(ConsoleFormatInfo) ? this : null;

        public static ConsoleFormatInfo ReadOnly(ConsoleFormatInfo cfi)
        {
            if (cfi == null)
            {
                throw new ArgumentNullException(nameof(cfi));
            }

            if (cfi.IsReadOnly)
            {
                return cfi;
            }

            ConsoleFormatInfo info = (ConsoleFormatInfo)cfi.MemberwiseClone();
            info._isReadOnly = true;
            return info;
        }

        private static ConsoleFormatInfo InitializeCurrentInfo()
        {
            bool supportsAnsi = 
                !Console.IsOutputRedirected &&
                DoesOperatingSystemSupportAnsi();

            return new ConsoleFormatInfo()
            {
                _isReadOnly = true,
                _supportsAnsiCodes = supportsAnsi
            };
        }

        private static bool DoesOperatingSystemSupportAnsi()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }

            // for Windows, check the console mode
            var stdOutHandle = Interop.GetStdHandle(Interop.STD_OUTPUT_HANDLE);
            if (!Interop.GetConsoleMode(stdOutHandle, out uint consoleMode))
            {
                return false;
            }

            return (consoleMode & Interop.ENABLE_VIRTUAL_TERMINAL_PROCESSING) == Interop.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }

        private void VerifyWritable()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException("Instance is read-only.");
            }
        }
    }
}
