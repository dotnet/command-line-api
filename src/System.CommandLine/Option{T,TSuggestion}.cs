// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    public class Option<T, TSuggestion> : Option<T>, IOption<TSuggestion>
        where TSuggestion : ISuggestionType<TSuggestion>, new()
    {
        public Option(
            string alias,
            string? description = null,
            bool enforceTextMatch = true)
            : this(alias, new Argument<T, TSuggestion>(), description, enforceTextMatch)
        { }

        public Option(
            string alias,
            Argument<T> argument,
            string? description = null,
            bool enforceTextMatch = true)
            : base(new[] { alias }, argument, description, enforceTextMatch)
        { }

        public Option(
            string[] aliases,
            Argument<T> argument,
            string? description = null,
            bool enforceTextMatch = true)
            : base(aliases, argument, description, enforceTextMatch)
        { }

        public IEnumerable<TSuggestion> GetGenericSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            return this.GetGenericSuggestions(EnforceTextMatch, Children, parseResult, textToMatch);
        }
    }
}
