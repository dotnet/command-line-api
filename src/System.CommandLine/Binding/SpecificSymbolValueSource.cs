// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class SpecificSymbolValueSource : IValueSource
    {
        public SpecificSymbolValueSource(IValueDescriptor valueDescriptor)
        {
            symbolValueSource = new CurrentSymbolResultValueSource();
            ValueDescriptor = valueDescriptor;
        }

        private readonly CurrentSymbolResultValueSource symbolValueSource;

        public IValueDescriptor ValueDescriptor { get; }

        public bool TryGetValue(IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object boundValue)
        {
            return symbolValueSource.TryGetValue(ValueDescriptor,
                bindingContext, out boundValue);
        }
    }
}
