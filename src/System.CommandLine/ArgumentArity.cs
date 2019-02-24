// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace System.CommandLine
{
    public class ArgumentArity : IArgumentArity
    {
        public ArgumentArity(int minimumNumberOfArguments, int maximumNumberOfArguments)
        {
            if (minimumNumberOfArguments < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumNumberOfArguments));
            }

            if (maximumNumberOfArguments < minimumNumberOfArguments)
            {
                throw new ArgumentException($"{nameof(maximumNumberOfArguments)} must be greater than or equal to {nameof(minimumNumberOfArguments)}");
            }

            MinimumNumberOfArguments = minimumNumberOfArguments;
            MaximumNumberOfArguments = maximumNumberOfArguments;
        }

        public int MinimumNumberOfArguments { get; set; }

        public int MaximumNumberOfArguments { get; set; }

        internal static FailedArgumentArityResult Validate(
            SymbolResult symbolResult,
            int minimumNumberOfArguments,
            int maximumNumberOfArguments)
        {
            if (symbolResult.Arguments.Count < minimumNumberOfArguments)
            {
                if (symbolResult.UseDefaultValue)
                {
                    return null;
                }

                return new FailedArgumentArityResult(symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult));
            }

            if (symbolResult.Arguments.Count > maximumNumberOfArguments)
            {
                if (maximumNumberOfArguments == 1)
                {
                    return new FailedArgumentArityResult(symbolResult.ValidationMessages.ExpectsOneArgument(symbolResult));
                }
                else
                {
                    return new FailedArgumentArityResult(symbolResult.ValidationMessages.ExpectsFewerArguments(symbolResult, maximumNumberOfArguments));
                }
            }

            return null;
        }

        public static IArgumentArity Zero => new ArgumentArity(0, 0);

        public static IArgumentArity ZeroOrOne => new ArgumentArity(0, 1);

        public static IArgumentArity ExactlyOne => new ArgumentArity(1, 1);

        public static IArgumentArity ZeroOrMore => new ArgumentArity(0, int.MaxValue);

        public static IArgumentArity OneOrMore => new ArgumentArity(1, int.MaxValue);

        internal static IArgumentArity Default(Type type, ISymbol symbol)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string))
            {
                return OneOrMore;
            }

            if (type == typeof(bool))
            {
                return ZeroOrOne;
            }

            if (type.IsValueType && 
                symbol is ICommand &&
                type.IsGenericType && 
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ZeroOrOne;
            }

            return ExactlyOne;
        }
    }
}
