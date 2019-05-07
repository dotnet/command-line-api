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
             symbol.Arguments().Any(a => a.ShouldShowHelp()));

        internal static bool ShouldShowHelp(
            this IArgument argument) =>
            argument != null &&
            !string.IsNullOrWhiteSpace(argument.Name) &&
            argument.Arity.MaximumNumberOfValues > 0;

        internal static IReadOnlyCollection<IArgument> Arguments(this ISymbol symbol)
        {
            switch (symbol)
            {
                case IOption option:
                    return new[]
                    {
                        option.Argument
                    };
                case ICommand command:
                    return command.Arguments;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static Token DefaultToken(this ICommand command)
        {
            return new Token(command.Name, TokenType.Option);
        }

        internal static Token DefaultToken(this IOption option)
        {
            var optionName = ((ISymbol) option).Name;

            var value = option.RawAliases.First(alias => alias.RemovePrefix() == optionName);

            return new Token(value, TokenType.Option);
        }
    }
}
