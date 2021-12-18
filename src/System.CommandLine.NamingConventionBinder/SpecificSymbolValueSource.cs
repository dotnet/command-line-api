// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

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
            case Option option:
                var optionResult = bindingContext?.ParseResult.FindResultFor(option);
                if (optionResult is not null)
                {
                    boundValue = optionResult.GetValueOrDefault();
                    return true;
                }
                break;
            case Argument argument:
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