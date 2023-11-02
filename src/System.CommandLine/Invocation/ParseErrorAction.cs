// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;

namespace System.CommandLine.Invocation;

/// <summary>
/// Provides command line output with error details in the case of a parsing error.
/// </summary>
public sealed class ParseErrorAction : SynchronousCliAction
{
    /// <summary>
    /// Indicates whether to show help along with error details when an error is found during parsing.
    /// </summary>
    /// <remarks>When set to <see langword="true" />, indicates that help will be shown along with parse error details. When set to false, help will not be shown.</remarks>
    public bool ShowHelp { get; set; } = true;

    /// <summary>
    /// Indicates whether to show typo suggestions along with error details when an error is found during parsing.
    /// </summary>
    /// <remarks>When set to <see langword="true" />, indicates that suggestions will be shown along with parse error details. When set to false, suggestions will not be shown.</remarks>
    public bool ShowTypoCorrections { get; set; } = true;

    /// <inheritdoc />
    public override int Invoke(ParseResult parseResult)
    {
        if (ShowTypoCorrections)
        {
            WriteTypoCorrectionSuggestions(parseResult);
        }

        WriteErrorDetails(parseResult);

        if (ShowHelp)
        {
            WriteHelp(parseResult);
        }

        return 1;
    }

    private static void WriteErrorDetails(ParseResult parseResult)
    {
        ConsoleHelpers.ResetTerminalForegroundColor();
        ConsoleHelpers.SetTerminalForegroundRed();

        foreach (var error in parseResult.Errors)
        {
            parseResult.Configuration.Error.WriteLine(error.Message);
        }

        parseResult.Configuration.Error.WriteLine();

        ConsoleHelpers.ResetTerminalForegroundColor();
    }

    private static void WriteHelp(ParseResult parseResult)
    {
        // Find the most proximate help option (if any) and invoke its action.
        var availableHelpOptions =
            parseResult
                .CommandResult
                .RecurseWhileNotNull(r => r.Parent as CommandResult)
                .Select(r => r.Command.Options.OfType<HelpOption>().FirstOrDefault());

        if (availableHelpOptions.FirstOrDefault(o => o is not null) is { Action: not null } helpOption)
        {
            switch (helpOption.Action)
            {
                case SynchronousCliAction syncAction:
                    syncAction.Invoke(parseResult);
                    break;

                case AsynchronousCliAction asyncAction:
                    asyncAction.InvokeAsync(parseResult, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
            }
        }
    }

    private static void WriteTypoCorrectionSuggestions(ParseResult parseResult)
    {
        var unmatchedTokens = parseResult.UnmatchedTokens;

        for (var i = 0; i < unmatchedTokens.Count; i++)
        {
            var token = unmatchedTokens[i];

            bool first = true;
            foreach (string suggestion in GetPossibleTokens(parseResult.CommandResult.Command, token))
            {
                if (first)
                {
                    parseResult.Configuration.Output.WriteLine(LocalizationResources.SuggestionsTokenNotMatched(token));
                    first = false;
                }

                parseResult.Configuration.Output.WriteLine(suggestion);
            }
        }

        parseResult.Configuration.Output.WriteLine();

        static IEnumerable<string> GetPossibleTokens(CliCommand targetSymbol, string token)
        {
            if (targetSymbol is { HasOptions: false, HasSubcommands: false })
            {
                return Array.Empty<string>();
            }

            IEnumerable<string> possibleMatches = targetSymbol
                                                  .Children
                                                  .Where(x => !x.Hidden && x is CliOption or CliCommand)
                                                  .Select(symbol =>
                                                  {
                                                      AliasSet? aliasSet = symbol is CliOption option ? option._aliases : ((CliCommand)symbol)._aliases;

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
                   .Select(possibleMatch => (possibleMatch, distance: GetDistance(token, possibleMatch)))
                   .Where(tuple => tuple.distance <= MaxLevenshteinDistance)
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

        static int GetStartsWithDistance(string first, string second)
        {
            int i;
            for (i = 0; i < first.Length && i < second.Length && first[i] == second[i]; i++)
            {
            }

            return i;
        }

        //Based on https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/
        static int GetDistance(string first, string second)
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

    private const int MaxLevenshteinDistance = 3;
}