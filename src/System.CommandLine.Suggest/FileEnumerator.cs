// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public static class  FileEnumerator
    {
        public static IEnumerable<string> EnumerateFilesWithoutExtension(DirectoryInfo path)
        {
            if (path == null || !path.Exists)
            {
                return Array.Empty<string>();
            }

            return path.EnumerateFiles()
                .Select(p => GetCommandName(p.FullName));
        }

        // Tool commands can contain dots (Example.Tools.Console), so only an .exe extension is removed.
        internal static string GetCommandName(string executablePath)
        {
            var fileName = Path.GetFileName(executablePath);

            return fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                       ? fileName.Substring(0, fileName.Length - ".exe".Length)
                       : fileName;
        }
    }
}
