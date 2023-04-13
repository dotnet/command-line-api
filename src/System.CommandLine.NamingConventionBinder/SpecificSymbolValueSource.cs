// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

internal class SpecificSymbolValueSource : IValueSource
{
    private readonly CliSymbol _symbol;

    public SpecificSymbolValueSource(CliSymbol symbol)
    {
        _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
    }

    public bool TryGetValue(IValueDescriptor valueDescriptor,
        BindingContext? bindingContext,
        out object? boundValue)
    {
        switch (_symbol)
        {
            case CliOption option:
                var optionResult = bindingContext?.ParseResult.GetResult(option);
                if (optionResult is not null)
                {
                    boundValue = optionResult.GetValueOrDefault<object>();
                    return true;
                }
                break;
            case CliArgument argument:
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