// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace System.CommandLine;

public static partial class Handler
{
    private static T? GetValueForHandlerParameter<T>(
        IValueDescriptor[] symbols,
        ref int index,
        InvocationContext context)
    {
        if (symbols.Length > index &&
            symbols[index] is IValueDescriptor<T> symbol)
        {
            index++;

            if (symbol is IValueSource valueSource && 
                valueSource.TryGetValue(symbol, context.BindingContext, out var boundValue) &&
                boundValue is T value)
            {
                return value;
            }
            else
            {
                return context.ParseResult.GetValueFor(symbol);
            }
        }

        var service = context.BindingContext.GetService(typeof(T));

        if (service is null)
        {
            throw new ArgumentException($"Service not found for type {typeof(T)}.");
        }

        return (T)service;
    }
}