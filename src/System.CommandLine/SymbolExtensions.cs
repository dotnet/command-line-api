// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    internal static class SymbolExtensions
    {
        public static bool IsHidden(this ISymbol symbol) =>
            string.IsNullOrWhiteSpace(symbol.Description);

        internal static bool HasArguments(this ISymbol symbol) =>
            symbol.Argument != null &&
            symbol.Argument != Argument.None;

        internal static bool HasHelp(this ISymbol symbol) =>
            symbol.Argument != null &&
            symbol.Argument.HasHelp;

        internal static string Token(this ISymbol symbol) => symbol.RawAliases.First(alias => alias.RemovePrefix() == symbol.Name);
    }
}
