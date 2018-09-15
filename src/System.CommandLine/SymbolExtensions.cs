// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal static class SymbolExtensions
    {
        public static bool IsHidden(this Symbol symbol) =>
            string.IsNullOrWhiteSpace(symbol.Description);
    }
}
