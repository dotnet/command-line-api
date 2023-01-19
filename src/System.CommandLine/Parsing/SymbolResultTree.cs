// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree
    {
        private readonly Dictionary<Symbol, SymbolResult> _symbolResults;
        private readonly LocalizationResources _localizationResources;

        internal SymbolResultTree(
            Dictionary<Symbol, SymbolResult> symbolResults,
            LocalizationResources localizationResources)
        {
            _symbolResults = symbolResults;
            _localizationResources = localizationResources;
        }

        internal LocalizationResources LocalizationResources => _localizationResources;

        internal ArgumentResult? FindResultFor(Argument argument)
            => _symbolResults.TryGetValue(argument, out SymbolResult? result) ? (ArgumentResult)result : default;

        internal CommandResult? FindResultFor(Command command)
            => _symbolResults.TryGetValue(command, out SymbolResult? result) ? (CommandResult)result : default;

        internal OptionResult? FindResultFor(Option option)
            => _symbolResults.TryGetValue(option, out SymbolResult? result) ? (OptionResult)result : default;

        internal IEnumerable<SymbolResult> GetChildren(SymbolResult parent)
        {
            foreach (KeyValuePair<Symbol, SymbolResult> pair in _symbolResults)
            {
                if (ReferenceEquals(parent, pair.Value.Parent))
                {
                    yield return pair.Value;
                }
            }
        }
    }
}
