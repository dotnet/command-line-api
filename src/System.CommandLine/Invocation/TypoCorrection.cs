// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class TypoCorrectionAction : CliAction
    {
        public override int Invoke(InvocationContext context)
            => ProvideSuggestions(context);

        public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(ProvideSuggestions(context));

        private static int ProvideSuggestions(InvocationContext context)
        {
            ParseResult result = context.ParseResult;
            IConsole console = context.Console;
            int maxLevenshteinDistance = result.Configuration.MaxLevenshteinDistance;

            var unmatchedTokens = result.UnmatchedTokens;
            for (var i = 0; i < unmatchedTokens.Count; i++)
            {
                var token = unmatchedTokens[i];

                bool first = true;
                foreach (string suggestion in GetPossibleTokens(result.CommandResult.Command, token, maxLevenshteinDistance))
                {
                    if (first)
                    {
                        console.Out.WriteLine(LocalizationResources.SuggestionsTokenNotMatched(token));
                        first = false;
                    }

                    console.Out.WriteLine(suggestion);
                }
            }

            return 0;
        }

        private static IEnumerable<string> GetPossibleTokens(Command targetSymbol, string token, int maxLevenshteinDistance)
        {
            if (!targetSymbol.HasOptions && !targetSymbol.HasSubcommands)
            {
                return Array.Empty<string>();
            }

            IEnumerable<string> possibleMatches = targetSymbol
                .Children
                .Where(x => !x.IsHidden && x is Option or Command)
                .Select(symbol =>
                {
                    AliasSet? aliasSet = symbol is Option option ? option._aliases : ((Command)symbol)._aliases; 

                    if (aliasSet is null)
                    {
                        return symbol.Name;
                    }

                    return new[] { symbol.Name }.Concat(aliasSet)
                        .OrderBy(x => GetDistance(token, x))
                        .ThenByDescending(x => GetStartsWithDistance(token, x))
                        .First();
                });
            
            int? bestDistance = null;
            return possibleMatches
                .Select(possibleMatch => (possibleMatch, distance:GetDistance(token, possibleMatch)))
                .Where(tuple => tuple.distance <= maxLevenshteinDistance)
                .OrderBy(tuple => tuple.distance)
                .ThenByDescending(tuple => GetStartsWithDistance(token, tuple.possibleMatch))
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

        private static int GetStartsWithDistance(string first, string second)
        {
            int i;
            for (i = 0; i < first.Length && i < second.Length && first[i] == second[i]; i++)
            { }
            return i;
        }

        //Based on https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/
        private static int GetDistance(string first, string second)
        {
            // Validate parameters
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }


            // Get the length of both.  If either is 0, return
            // the length of the other, since that number of insertions
            // would be required.

            int n = first.Length, m = second.Length;
            if (n == 0) return m;
            if (m == 0) return n;


            // Rather than maintain an entire matrix (which would require O(n*m) space),
            // just store the current row and the next row, each of which has a length m+1,
            // so just O(m) space. Initialize the current row.

            int curRow = 0, nextRow = 1;
            int[][] rows = { new int[m + 1], new int[m + 1] };

            for (int j = 0; j <= m; ++j)
            {
                rows[curRow][j] = j;
            }

            // For each virtual row (since we only have physical storage for two)
            for (int i = 1; i <= n; ++i)
            {
                // Fill in the values in the row
                rows[nextRow][0] = i;
                for (int j = 1; j <= m; ++j)
                {
                    int dist1 = rows[curRow][j] + 1;
                    int dist2 = rows[nextRow][j - 1] + 1;
                    int dist3 = rows[curRow][j - 1] + (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                    rows[nextRow][j] = Math.Min(dist1, Math.Min(dist2, dist3));
                }


                // Swap the current and next rows
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }

            // Return the computed edit distance
            return rows[curRow][m];
        }
    }
}
