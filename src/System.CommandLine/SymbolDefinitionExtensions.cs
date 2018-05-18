// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal static class SymbolDefinitionExtensions
    {
        public static bool IsHidden(this SymbolDefinition symbolDefinition) =>
            string.IsNullOrWhiteSpace(symbolDefinition.Description);
    }
}
