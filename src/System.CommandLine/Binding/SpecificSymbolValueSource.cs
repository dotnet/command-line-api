// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Binding
{
    internal class SpecificSymbolValueSource : IValueSource
    {
        public SpecificSymbolValueSource(IValueDescriptor valueDescriptor)
        {
            ValueDescriptor = valueDescriptor ?? throw new ArgumentNullException(nameof(valueDescriptor));
        }

        public IValueDescriptor ValueDescriptor { get; }

        public bool TryGetValue(IValueDescriptor valueDescriptor,
                                BindingContext? bindingContext,
                                out object? boundValue)
        {
            var specificDescriptor = ValueDescriptor;
            switch (specificDescriptor)
            {
                case IOption option:
                    var optionResult = bindingContext?.ParseResult.FindResultFor(option);
                    if (optionResult is not null)
                    {
                        boundValue = optionResult.GetValueOrDefault();
                        return true;
                    }
                    break;
                case IArgument argument:
                    var argumentResult = bindingContext?.ParseResult.FindResultFor(argument);
                    if (argumentResult is not null)
                    {
                        boundValue = argumentResult.GetValueOrDefault();
                        return true;
                    }
                    break;
            }

            boundValue = null;
            return false;
        }
    }
}
