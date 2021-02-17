// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine
{
    internal static class DebugAssert
    {
        [Conditional("DEBUG")]
        public static void ThrowIf(bool condition, string message)
        {
            if (condition)
            {
                Throw(message);
            }
        }
        
        [Conditional("DEBUG")]
        public static void Throw(string message)
        {
            throw new Exception(message);
        }
    }
}