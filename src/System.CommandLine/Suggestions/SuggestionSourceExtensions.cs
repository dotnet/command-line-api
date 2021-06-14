// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Suggestions
{
    public static class SuggestionSourceExtensions
    {
        public static IEnumerable<TSuggestion> GetGenericSuggestions<TSuggestion>(
            this ISuggestionSource<TSuggestion> source,
            bool enforceTextMatch,
            ISymbolSet children,
            ParseResult? parseResult = null,
            string? textToMatch = null)
            where TSuggestion : ISuggestionType<TSuggestion>, new()
        {
            var suggestions = new HashSet<TSuggestion>();

            if (enforceTextMatch)
            {
                textToMatch ??= "";
            }
            else
            {
                textToMatch = "";
            }

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];

                switch (child)
                {
                    case IIdentifierSymbol identifier when !child.IsHidden:
                        foreach (var alias in identifier.Aliases)
                        {
                            if (alias is { } suggestion &&
                                suggestion.ContainsCaseInsensitive(textToMatch))
                            {
                                suggestions.Add(new TSuggestion().Build(parseResult, suggestion));
                            }
                        }
                        break;
                    case IArgument<TSuggestion> argument:
                        foreach (var suggestion in argument.GetGenericSuggestions(parseResult, textToMatch))
                        {
                            if (suggestion is { } &&
                                suggestion.DoesTextMatch(textToMatch))
                            {
                                suggestions.Add(suggestion);
                            }
                        }
                        break;
                    case IArgument argument:
                        foreach (var suggestion in argument.GetSuggestions(parseResult, textToMatch))
                        {
                            if (suggestion is { } &&
                                suggestion.ContainsCaseInsensitive(textToMatch))
                            {
                                suggestions.Add(new TSuggestion().Build(parseResult, suggestion));
                            }
                        }
                        break;
                }
            }

            return suggestions
                .OrderBy(
                    s => s,
                    Comparer<TSuggestion>.Create((sug1, sug2) => sug1.CompareToWithTextToMatch(sug2, textToMatch)))
                .ThenBy(s => s);
        }
    }
}
