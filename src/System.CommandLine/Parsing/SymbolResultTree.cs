// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        internal List<ParseError>? Errors;
        internal List<Token>? UnmatchedTokens;

        internal SymbolResultTree(List<string>? tokenizeErrors)
        {
            if (tokenizeErrors is not null)
            {
                Errors = new List<ParseError>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    Errors.Add(new ParseError(tokenizeErrors[i]));
                }
            }
        }

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

        internal void AddError(ParseError parseError) => (Errors ??= new()).Add(parseError);

        internal void InsertFirstError(ParseError parseError) => (Errors ??= new()).Insert(0, parseError);

        internal void AddUnmatchedToken(Token token) => (UnmatchedTokens ??= new()).Add(token);
    }
}
