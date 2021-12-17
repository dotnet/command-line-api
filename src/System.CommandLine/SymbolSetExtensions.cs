// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;

namespace System.CommandLine
{
    internal static class SymbolSetExtensions
    {
        public static bool HasAnyOfType<T>(this SymbolSet source)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (source[i] is T)
                {
                    return true;
                }
            }

            return false;
        }
    }
}