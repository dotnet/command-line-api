// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

internal class SpecificSymbolValueSource : IValueSource
{
    private readonly Symbol _symbol;

    public SpecificSymbolValueSource(Symbol symbol)
    {
        _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
    }

    public bool TryGetValue(IValueDescriptor valueDescriptor,
        BindingContext? bindingContext,
        out object? boundValue)
    {
        switch (_symbol)
        {
            case Option option:
                var optionResult = bindingContext?.ParseResult.GetResult(option);
                if (optionResult is not null)
                {
                    boundValue = optionResult.GetValueOrDefault<object>();
                    return true;
                }
                break;
            case Argument argument:
                var argumentResult = bindingContext?.ParseResult.GetResult(argument);
                if (argumentResult is not null)
                {
                    boundValue = argumentResult.GetValueOrDefault<object>();
                    return true;
                }
                break;
        }

        boundValue = null;
        return false;
    }
}