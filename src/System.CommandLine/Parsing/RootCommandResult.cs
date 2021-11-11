// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal class RootCommandResult : CommandResult
    {
        private readonly Dictionary<ISymbol, SymbolResult> _symbolResults;

        public RootCommandResult(
            ICommand command,
            Token token,
            Dictionary<ISymbol, SymbolResult> symbolResults) : base(command, token)
        {
            _symbolResults = symbolResults;
        }

        internal override RootCommandResult Root => this;

        public override ArgumentResult? FindResultFor(IArgument argument)
        {
            if (_symbolResults.TryGetValue(argument, out var result) &&
                result is ArgumentResult argumentResult)
            {
                return argumentResult;
            }

            return default;
        }

        public override CommandResult? FindResultFor(ICommand command)
        {
            if (_symbolResults.TryGetValue(command, out var result) &&
                result is CommandResult commandResult)
            {
                return commandResult;
            }

            return default;
        }

        public override OptionResult? FindResultFor(IOption option)
        {
            if (_symbolResults.TryGetValue(option, out var result) &&
                result is OptionResult optionResult)
            {
                return optionResult;
            }

            return default;
        }

        internal SymbolResult? FindResultForSymbol(ISymbol symbol)
        {
            switch (symbol)
            {
                case IArgument argument:
                    return FindResultFor(argument);
                case ICommand command:
                    return FindResultFor(command);
                case IOption option:
                    return FindResultFor(option);
                default:
                    throw new ArgumentException($"Unsupported symbol type: {symbol.GetType()}");
            }
        }

        internal void AddToSymbolMap(SymbolResult result)
        {
            _symbolResults.TryAdd(result.Symbol, result);
        }

        // FIX: (RootCommandResult) delete these:

        internal IReadOnlyCollection<ArgumentResult> AllArgumentResults => _symbolResults.Values.OfType<ArgumentResult>().ToArray();

        internal IReadOnlyCollection<OptionResult> AllOptionResults => _symbolResults.Values.OfType<OptionResult>().ToArray();
    }
}
