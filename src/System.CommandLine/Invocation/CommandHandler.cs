// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Invocation;

public static partial class CommandHandler
{
    private static T? GetParsedValueOrService<T>(
        IValueDescriptor[] symbols,
        ref int index,
        InvocationContext context)
    {
        if (symbols.Length > index &&
            symbols[index] is IValueDescriptor<T> symbol1)
        {
            index++;
            return context.ParseResult.GetValueFor(symbol1);
        }
        else
        {
            var service = context.BindingContext.GetService(typeof(T));

            if (service is null)
            {
                throw new ArgumentException($"Service not found for type {typeof(T)}.");
            }

            return (T)service;
        }
    }
}