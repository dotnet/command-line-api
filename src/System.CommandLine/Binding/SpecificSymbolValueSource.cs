// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class SpecificSymbolValueSource : IValueSource
    {
        public SpecificSymbolValueSource(ISymbol symbol)
        {
            Symbol = symbol;
        }

        public ISymbol Symbol { get; }

        public bool TryGetValue(IValueDescriptor valueDescriptor, BindingContext bindingContext, out object value)
        {
            var optionResult = bindingContext.ParseResult.FindResultFor(Symbol);

            if (optionResult == null)
            {
                value = null;
                return false;
            }
            else
            {
                value = optionResult.GetValueOrDefault();
                return true;
            }
        }
    }
}
