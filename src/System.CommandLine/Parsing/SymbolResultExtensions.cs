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
                                 .FlattenBreadthFirst(o => o.SymbolResultTree.GetChildren(o)))
            {
                yield return item;
            }
        }

    }
}