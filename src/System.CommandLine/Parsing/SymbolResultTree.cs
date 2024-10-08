// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<CliSymbol, CliSymbolResultInternal>
    {
        private readonly CliCommand _rootCommand;
        internal List<CliDiagnostic>? Errors;
        // TODO: unmatched tokens
        /*
        internal List<CliToken>? UnmatchedTokens;
        */

        // TODO: Looks like this is a SymboNode/linked list because a symbol may appear multiple
        // places in the tree and multiple symbols will have the same short name. The question is 
        // whether creating the multiple node instances is faster than just using lists. Could well be.
        internal SymbolResultTree(
            CliCommand rootCommand,
            List<string>? tokenizeErrors)
        {
            _rootCommand = rootCommand;

            if (tokenizeErrors is not null)
            {
                Errors = new List<CliDiagnostic>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    Errors.Add(new CliDiagnostic(new("", "",
                        tokenizeErrors[i], CliDiagnosticSeverity.Warning, null), [],
                        cliSymbolResult: null));
                }
            }
        }

        internal int ErrorCount => Errors?.Count ?? 0;

        internal CliArgumentResultInternal? GetResultInternal(CliArgument argument)
            => TryGetValue(argument, out CliSymbolResultInternal? result) ? (CliArgumentResultInternal)result : default;

        internal CliCommandResultInternal? GetResultInternal(CliCommand command)
            => TryGetValue(command, out var result) ? (CliCommandResultInternal)result : default;

        internal CliOptionResultInternal? GetResultInternal(CliOption option)
            => TryGetValue(option, out CliSymbolResultInternal? result) ? (CliOptionResultInternal)result : default;

        // TODO: Determine how this is used. It appears to be O^n in the size of the tree and so if it is called multiple times, we should reconsider to avoid O^(N*M)
        internal IEnumerable<CliSymbolResultInternal> GetChildren(CliSymbolResultInternal parent)
        {
            // Argument can't have children
            if (parent is not CliArgumentResultInternal)
            {
                foreach (KeyValuePair<CliSymbol, CliSymbolResultInternal> pair in this)
                {
                    if (ReferenceEquals(parent, pair.Value.Parent))
                    {
                        yield return pair.Value;
                    }
                }
            }
        }

        internal IReadOnlyDictionary<CliSymbol, CliValueResult> BuildValueResultDictionary()
        {
            var dict = new Dictionary<CliSymbol, CliValueResult>();
            foreach (KeyValuePair<CliSymbol, CliSymbolResultInternal> pair in this)
            {
                var result = pair.Value;
                if (result is CliOptionResultInternal optionResult)
                {
                    dict.Add(pair.Key, optionResult.ValueResult);
                    continue;
                }
                if (result is CliArgumentResultInternal argumentResult)
                {
                    dict.Add(pair.Key, argumentResult.ValueResult);
                    continue;
                }
            }
            return dict;
        }

        internal void AddError(CliDiagnostic CliDiagnostic) => (Errors ??= new()).Add(CliDiagnostic);
        internal void InsertFirstError(CliDiagnostic CliDiagnostic) => (Errors ??= new()).Insert(0, CliDiagnostic);

        internal void AddUnmatchedToken(CliToken token, CliCommandResultInternal commandResult, CliCommandResultInternal rootCommandResult)
        {
            /*
            // TODO: unmatched tokens
            (UnmatchedTokens ??= new()).Add(token);

            if (commandResult.Command.TreatUnmatchedTokensAsErrors)
            {
                if (commandResult != rootCommandResult && !rootCommandResult.Command.TreatUnmatchedTokensAsErrors)
                {
                    return;
                }

            */
            AddError(new CliDiagnostic(new("", "", LocalizationResources.UnrecognizedCommandOrArgument(token.Value), CliDiagnosticSeverity.Warning, null), []));
            /*
            }
            */
        }
    }
}
