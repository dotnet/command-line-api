// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal class RootCommandResult : CommandResult
    {
        private readonly Dictionary<IArgument, ArgumentResult> _allArgumentResults;
        private readonly Dictionary<ICommand, CommandResult> _allCommandResults;
        private readonly Dictionary<IOption, OptionResult> _allOptionResults;

        public RootCommandResult(
            ICommand command,
            Token token,
            Dictionary<IArgument, ArgumentResult> _allArgumentResults,
            Dictionary<ICommand, CommandResult> _allCommandResults,
            Dictionary<IOption, OptionResult> _allOptionResults) : base(command, token)
        {
            this._allArgumentResults = _allArgumentResults;
            this._allCommandResults = _allCommandResults;
            this._allOptionResults = _allOptionResults;
        }

        internal override RootCommandResult Root => this;

        public override ArgumentResult? FindResultFor(IArgument argument)
        {
            _allArgumentResults.TryGetValue(argument, out var result);

            return result;
        }

        public override CommandResult? FindResultFor(ICommand command)
        {
            _allCommandResults.TryGetValue(command, out var result);

            return result;
        }

        public override OptionResult? FindResultFor(IOption option)
        {
            _allOptionResults.TryGetValue(option, out var result);

            return result;
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
            switch (result)
            {
                case ArgumentResult argumentResult:
                    _allArgumentResults.TryAdd(argumentResult.Argument, argumentResult);
                    break;
                case CommandResult commandResult:
                    _allCommandResults.TryAdd(commandResult.Command, commandResult);
                    break;
                case OptionResult optionResult:
                    _allOptionResults.TryAdd(optionResult.Option, optionResult);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(SymbolResult)} type: {result.GetType()}");
            }
        }

        internal IReadOnlyCollection<ArgumentResult> AllArgumentResults => _allArgumentResults.Values;

        internal IReadOnlyCollection<OptionResult> AllOptionResults => _allOptionResults.Values;
    }
}
