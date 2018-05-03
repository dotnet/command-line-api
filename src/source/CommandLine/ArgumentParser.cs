// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.ArgumentArity;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentParser
    {
        private readonly List<Suggest> suggestionSources = new List<Suggest>();

        public ArgumentParser(
            ArgumentArity argumentArity,
            ConvertArgument convert = null)
        {
            ArgumentArity = argumentArity;
            ConvertArguments = convert;
        }

        public ArgumentArity ArgumentArity { get; }

        internal ConvertArgument ConvertArguments{get;}

        public void AddSuggestionSource(Suggest suggest)
        {
            suggestionSources.Add(suggest);
        }

        public virtual IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            foreach (var suggestionSource in suggestionSources)
            {
                foreach (var suggestion in suggestionSource(parseResult, position))
                {
                    yield return suggestion;
                }
            }
        }

        public ArgumentParseResult Parse(ParsedSymbol symbol)
        {
            if (ConvertArguments != null)
            {
                return ConvertArguments(symbol);
            }

            switch (ArgumentArity)
            {
                case Zero:
                    return ArgumentParseResult.Success((string) null);
                case One:
                    return ArgumentParseResult.Success(symbol.Arguments.Single());
                case Many:
                default:
                    return ArgumentParseResult.Success(symbol.Arguments);
            }
        }
    }
}
