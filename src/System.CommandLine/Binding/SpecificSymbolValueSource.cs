// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class SymbolValueSource : IValueSource
    {
        public SymbolValueSource(ISymbol symbol)
        {
            Symbol = symbol;
        }

        public ISymbol Symbol { get; }

        public bool TryGetValue(
            IValueDescriptor valueDescriptor, 
            BindingContext bindingContext, 
            out object boundValue)
        {
            var symbolResult = bindingContext.ParseResult.FindResultFor(Symbol);

            boundValue = symbolResult == null
                        ? Symbol.GetDefaultValue()
                        : symbolResult.GetValueOrDefault();

            return true;
        }
    }
}
