// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        private readonly LocalizationResources _localizationResources;
        internal List<ParseError>? Errors;

        internal SymbolResultTree(LocalizationResources localizationResources, List<string>? tokenizeErrors)
        {
            _localizationResources = localizationResources;

            if (tokenizeErrors is not null)
            {
                Errors = new List<ParseError>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    Errors.Add(new ParseError(tokenizeErrors[i]));
                }
            }
        }

        internal LocalizationResources LocalizationResources => _localizationResources;

        internal int ErrorCount => Errors?.Count ?? 0;

        internal ArgumentResult? FindResultFor(Argument argument)
            => TryGetValue(argument, out SymbolResult? result) ? (ArgumentResult)result : default;

        internal CommandResult? FindResultFor(Command command)
            => TryGetValue(command, out SymbolResult? result) ? (CommandResult)result : default;

        internal OptionResult? FindResultFor(Option option)
            => TryGetValue(option, out SymbolResult? result) ? (OptionResult)result : default;

        internal IEnumerable<SymbolResult> GetChildren(SymbolResult parent)
        {
            if (parent is not ArgumentResult)
            {
                foreach (KeyValuePair<Symbol, SymbolResult> pair in this)
                {
                    if (ReferenceEquals(parent, pair.Value.Parent))
                    {
                        yield return pair.Value;
                    }
                }
            }
        }

        internal void ReportError(ParseError parseError) => (Errors ??= new()).Add(parseError);

        internal void InsertError(int index, ParseError parseError) => (Errors ??= new()).Insert(index, parseError);
    }
}
