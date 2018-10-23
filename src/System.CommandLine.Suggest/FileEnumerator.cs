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
                .Select(p => Path.GetFileNameWithoutExtension(p.FullName));
        }
    }
}
