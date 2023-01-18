// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class RootCommandResult : CommandResult
    {
        private readonly Dictionary<Symbol, SymbolResult> _symbolResults;
        private readonly LocalizationResources _localizationResources;

        public RootCommandResult(
            Command command,
            Token token,
            Dictionary<Symbol, SymbolResult> symbolResults,
            LocalizationResources localizationResources) : base(command, token)
        {
            _symbolResults = symbolResults;
            _localizationResources = localizationResources;
        }

        public override LocalizationResources LocalizationResources => _localizationResources;

        public override ArgumentResult? FindResultFor(Argument argument)
            => _symbolResults.TryGetValue(argument, out SymbolResult? result) ? (ArgumentResult)result : default;

        public override CommandResult? FindResultFor(Command command)
            => _symbolResults.TryGetValue(command, out SymbolResult? result) ? (CommandResult)result : default;

        public override OptionResult? FindResultFor(Option option)
            => _symbolResults.TryGetValue(option, out SymbolResult? result) ? (OptionResult)result : default;
    }
}
