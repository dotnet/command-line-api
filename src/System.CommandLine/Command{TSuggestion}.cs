// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    ///<inheritdoc/>
    public class Command<TSuggestion> : Command, ICommand<TSuggestion>
        where TSuggestion : ISuggestionType<TSuggestion>, new() 
    {
        public Command(string name, string? description = null, bool enforceTextMatch = true)
            : base(name, description, enforceTextMatch)
        { }

        public IEnumerable<TSuggestion> GetGenericSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            return this.GetGenericSuggestions(EnforceTextMatch, Children, parseResult, textToMatch);
        }
    }
}
