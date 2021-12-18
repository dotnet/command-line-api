// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal class RootCommandResult : CommandResult
    {
        private readonly Dictionary<Symbol, SymbolResult> _symbolResults;

        public RootCommandResult(
            Command command,
            Token token,
            Dictionary<Symbol, SymbolResult> symbolResults) : base(command, token)
        {
            _symbolResults = symbolResults;
        }

        internal override RootCommandResult Root => this;

        public override ArgumentResult? FindResultFor(Argument argument)
        {
            if (_symbolResults.TryGetValue(argument, out var result) &&
                result is ArgumentResult argumentResult)
            {
                return argumentResult;
            }

            return default;
        }

        public override CommandResult? FindResultFor(Command command)
        {
            if (_symbolResults.TryGetValue(command, out var result) &&
                result is CommandResult commandResult)
            {
                return commandResult;
            }

            return default;
        }

        public override OptionResult? FindResultFor(Option option)
        {
            if (_symbolResults.TryGetValue(option, out var result) &&
                result is OptionResult optionResult)
            {
                return optionResult;
            }

            return default;
        }

        internal SymbolResult? FindResultForSymbol(Symbol symbol)
        {
            switch (symbol)
            {
                case Argument argument:
                    return FindResultFor(argument);
                case Command command:
                    return FindResultFor(command);
                case Option option:
                    return FindResultFor(option);
                default:
                    throw new ArgumentException($"Unsupported symbol type: {symbol.GetType()}");
            }
        }
    }
}
