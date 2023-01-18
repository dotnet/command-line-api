// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal static class SymbolResultExtensions
    {
        internal static IEnumerable<SymbolResult> AllSymbolResults(this CommandResult commandResult)
        {
            yield return commandResult;

            foreach (var item in commandResult
                                 .Children
                                 .FlattenBreadthFirst(o => o.GetChildren(o)))
            {
                yield return item;
            }
        }

        internal static Token Token(this SymbolResult symbolResult)
        {
            return symbolResult switch
            {
                CommandResult commandResult => commandResult.Token,
                OptionResult optionResult => optionResult.Token ?? CreateImplicitToken(optionResult.Option),
                _ => throw new ArgumentOutOfRangeException(nameof(symbolResult))
            };

            static Token CreateImplicitToken(Option option)
            {
                return new Token(option.GetLongestAlias(removePrefix: false), TokenType.Option, option, Parsing.Token.ImplicitPosition);
            }
        }
    }
}