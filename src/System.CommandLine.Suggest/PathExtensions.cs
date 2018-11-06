// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Suggest
{
    internal static class PathExtensions
    {
        public static string RemoveExeExtension(this string path)
        {
            if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(path.LastIndexOf(".exe", StringComparison.OrdinalIgnoreCase));
            }

            return path;
        }
    }
}
