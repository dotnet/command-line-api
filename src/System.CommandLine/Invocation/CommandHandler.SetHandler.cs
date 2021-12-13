// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Invocation;

public static partial class CommandHandler
{
    public static ICommandHandler SetHandler<T1, T2, T3>(
        this Command command,
        Action<T1, T2, T3> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);

                handle(value1,
                       value2,
                       value3);
            });

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
            return context.BindingContext.GetService<T>();
        }
    }
}