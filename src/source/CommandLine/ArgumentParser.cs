// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public abstract class ArgumentParser
    {
        private readonly List<Suggest> suggestionSources = new List<Suggest>();

        public void AddSuggetions(Suggest suggest)
        {
            suggestionSources.Add(suggest);
        }

        public virtual IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            foreach (var suggestionSource in suggestionSources)
            {
                foreach (var suggestion in suggestionSource(parseResult, position))
                {
                    yield return suggestion;
                }
            }
        }

        public abstract ArgumentParseResult Parse(ParsedSymbol value);
    }
}