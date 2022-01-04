// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using static System.Environment;

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
            var candidates = context.ParseResult
                                    .RootCommandResult
                                    .AllSymbolResults()
                                    .Where(r => r.Symbol.Parents.All(p => p is not Option))
                                    .Select(r => r.Symbol)
                                    .OfType<IValueDescriptor>()
                                    .ToArray();

            if (candidates.Any())
            {
                var candidatesDescription = string.Join(
                    NewLine,
                    candidates
                        .Where(c => typeof(T).IsAssignableFrom(c.ValueType))
                        .Select(c => c switch
                        {
                            Argument<T> argument => $"{nameof(Argument)}<{argument.ValueType.Name}> {argument.Name}",
                            Argument argument => $"{nameof(Argument)} {argument.Name}",
                            Option<T> option => $"{nameof(Option)}<{option.ValueType.Name}> {option.Aliases.First()}",
                            Option option => $"{nameof(Option)} {option.Aliases.First()}",
                            _ => throw new ArgumentOutOfRangeException(nameof(c))
                        }));

                throw new ArgumentException(
                    $"No binding target was provided to the handler for command '{context.ParseResult.CommandResult.Command.Name}' for the parameter at position {index}. Did you mean to pass one of these?{NewLine}{candidatesDescription}");
            }

            throw new ArgumentException($"Service not found for type {typeof(T)}.");
        }

        return (T)service;
    }
}