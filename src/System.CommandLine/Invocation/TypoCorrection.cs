// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Invocation
{
    internal class TypoCorrection
    {
        private readonly int _maxLevenshteinDistance;

        public TypoCorrection(int maxLevenshteinDistance)
        {
            if (maxLevenshteinDistance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevenshteinDistance));
            }

            _maxLevenshteinDistance = maxLevenshteinDistance;
        }

        public void ProvideSuggestions(ParseResult result, IConsole console)
        {
            for (var i = 0; i < result.UnmatchedTokens.Count; i++)
            {
                var token = result.UnmatchedTokens[i];
                var suggestions = GetPossibleTokens(result.CommandResult.Command, token).ToList();
                if (suggestions.Count > 0)
                {
                    console.Out.WriteLine(result.CommandResult.LocalizationResources.SuggestionsTokenNotMatched(token));
                    foreach(string suggestion in suggestions)
                    {
                        console.Out.WriteLine(suggestion);
                    }
                }
            }
        }

        private IEnumerable<string> GetPossibleTokens(Command targetSymbol, string token)
        {
            IEnumerable<string> possibleMatches = targetSymbol
                .Children
                .OfType<IdentifierSymbol>()
                .Where(x => !x.IsHidden)
                .Where(x => x.Aliases.Count > 0)
                .Select(symbol => 
                    symbol.Aliases
                        .Union(symbol.Aliases)
                        .OrderBy(x => TokenDistances.GetLevensteinDistance(token, x))
                        .ThenByDescending(x => TokenDistances.GetStartsWithDistance(token, x))
                        .First()
                );
            
            int? bestDistance = null;
            return possibleMatches
                .Select(possibleMatch => (possibleMatch, distance: TokenDistances.GetLevensteinDistance(token, possibleMatch)))
                .Where(tuple => tuple.distance <= _maxLevenshteinDistance)
                .OrderBy(tuple => tuple.distance)
                .ThenByDescending(tuple => TokenDistances.GetStartsWithDistance(token, tuple.possibleMatch))
                .TakeWhile(tuple =>
                {
                    var (_, distance) = tuple;
                    if (bestDistance is null)
                    {
                        bestDistance = distance;
                    }
                    return distance == bestDistance;
                })
                .Select(tuple => tuple.possibleMatch);
        }
    }
}
