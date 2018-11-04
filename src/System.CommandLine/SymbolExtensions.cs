// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    internal static class SymbolExtensions
    {
        internal static IEnumerable<string> ChildSymbolAliases(this ISymbol symbol) =>
            symbol.Children
                  .Where(s => !s.IsHidden)
                  .SelectMany(s => s.RawAliases);

        internal static bool ShouldShowHelp(this ISymbol symbol) =>
            !symbol.IsHidden &&
            symbol.Help != null ||
            (symbol.Argument != null &&
             symbol.Argument.ShouldShowHelp());

        internal static bool ShouldShowHelp(this IArgument argument) =>
            argument.Help != null &&
            argument.Arity.MaximumNumberOfArguments > 0;

        internal static string Token(this ISymbol symbol) => symbol.RawAliases.First(alias => alias.RemovePrefix() == symbol.Name);
    }
}
