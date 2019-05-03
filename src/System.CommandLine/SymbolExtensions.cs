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
            (!string.IsNullOrWhiteSpace(symbol.Name) ||
             !string.IsNullOrWhiteSpace(symbol.Description) ||
             symbol.Argument.ShouldShowHelp());

        internal static bool ShouldShowHelp(
            this IArgument argument) =>
            argument != null &&
            (!string.IsNullOrWhiteSpace(argument.Name) || string.IsNullOrWhiteSpace(argument.Description)) && 
            argument.Arity.MaximumNumberOfValues > 0;

        internal static Token DefaultToken(this ICommand command)
        {
            return new Token(command.Name, TokenType.Option);
        }

        internal static Token DefaultToken(this IOption option)
        {
            var value = option.RawAliases.First(alias => alias.RemovePrefix() == option.Name);

            return new Token(value, TokenType.Option);
        }
    }
}
