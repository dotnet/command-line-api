// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal static class SymbolResultExtensions
    {
        internal static IEnumerable<SymbolResult> AllSymbolResults(this SymbolResult symbolResult)
        {
            if (symbolResult == null)
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

        internal static Token Token(this SymbolResult symbolResult) =>
            symbolResult switch
            {
                CommandResult commandResult => commandResult.Token,
                OptionResult optionResult => optionResult.Token,
                _ => throw new ArgumentOutOfRangeException(nameof(symbolResult))
            };
    }
}
