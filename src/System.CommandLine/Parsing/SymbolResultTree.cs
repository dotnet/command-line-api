// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        private readonly LocalizationResources _localizationResources;

        internal SymbolResultTree(LocalizationResources localizationResources)
        {
            _localizationResources = localizationResources;
        }

        internal LocalizationResources LocalizationResources => _localizationResources;

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
    }
}
