// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Suggest
{
    public class FileEnumerator : IFileEnumerator
    {
        public IEnumerable<string> EnumerateFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(path);
        }
    }
}
