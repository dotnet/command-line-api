// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static class Exports
{
    // Exported native symbol. Builds a RootCommand and returns its Name as UTF-8.
    // When hosted as a native library there is no managed entry point
    // (Assembly.GetEntryAssembly() returns null) and Environment.GetCommandLineArgs()
    // reflects the host process, so RootCommand.ExecutableName falls back to the
    // AppContext value injected by the System.CommandLine build targets (the assembly name).
    //
    // Two-call protocol: pass a null buffer to get the required length, then call
    // again with a buffer of that size. Returns the number of UTF-8 bytes.
    [UnmanagedCallersOnly(EntryPoint = "get_executable_name", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int GetExecutableName(IntPtr buffer, int bufferLength)
    {
        string name = new RootCommand().Name;
        byte[] bytes = Encoding.UTF8.GetBytes(name);

        if (buffer != IntPtr.Zero && bufferLength >= bytes.Length)
        {
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
        }

        return bytes.Length;
    }
}
