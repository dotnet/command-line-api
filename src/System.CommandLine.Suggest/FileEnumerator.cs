// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public static class  FileEnumerator
    {
        public static IEnumerable<string> EnumerateFilesWithoutExtension(string path)
        {
            if (!Directory.Exists(path))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(path)
                .Select(p => Path.GetFileNameWithoutExtension(p));
        }
    }
}
