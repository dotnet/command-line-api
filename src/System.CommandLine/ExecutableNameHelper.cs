// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine
{
    internal static class ExecutableNameHelper
    {
        // Starting with .NET 11, Environment.GetCommandLineArgs()[0] is the name the apphost was
        // invoked with (https://github.com/dotnet/runtime/pull/131671) rather than the managed
        // ".dll" path. On Linux/macOS the apphost has no extension, so Path.GetFileNameWithoutExtension
        // would treat the last segment of a dotted name like "Contoso.Tool" as an extension and drop it
        // (issue #2850). Only strip the known host extensions.
        internal static string GetExecutableName(string path)
        {
#if NET
            ReadOnlySpan<char> fileName = Path.GetFileName(path.AsSpan());
#else
            string fileName = Path.GetFileName(path);
#endif

            if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName[..^4];
            }

            return fileName
#if NET
                .ToString()
#endif
                .Replace(" ", "");
        }
    }
}
