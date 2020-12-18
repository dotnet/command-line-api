// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal static class SymbolResultExtensions
    {
        internal static IEnumerable<SymbolResult> AllSymbolResults(this SymbolResult symbolResult)
        {
            if (symbolResult is null)
            {
                throw new ArgumentNullException(nameof(symbolResult));
            }

            yield return symbolResult;

            foreach (var item in symbolResult
                                 .Children
                                 .FlattenBreadthFirst(o => o.Children))
            {
                yield return item;
            }
        }
        
        internal static IEnumerable<SymbolResult> AllSymbolResults2(this SymbolResult symbolResult)
        {
            var tokenPositions = symbolResult.AllSymbolResults()
                                                   .Select(r => r switch
                                                   {
                                                       CommandResult c => (symbol:r, pos: c.Token.Position),
                                                       OptionResult o => (symbol: r, pos: o.Token?.Position),
                                                       ArgumentResult a => (symbol: r, pos: a.Tokens.FirstOrDefault()?.Position ?? -1),
                                                       _ => (r, -1)
                                                   })
                                                   .OrderBy(x => x.pos)
                                                   .Select(x => x.symbol);

            return tokenPositions;
        }

        internal static Token Token(this SymbolResult symbolResult)
        {
            return symbolResult switch
            {
                CommandResult commandResult => commandResult.Token,
                OptionResult optionResult => optionResult.Token ??
                                             CreateImplicitToken(optionResult.Option),
                _ => throw new ArgumentOutOfRangeException(nameof(symbolResult))
            };

            Token CreateImplicitToken(IOption option)
            {
                var optionName = option.Name;

                var defaultAlias = option.Aliases.First(alias => alias.RemovePrefix() == optionName);

                return new ImplicitToken(defaultAlias, TokenType.Option);
            }
        }
    }
}