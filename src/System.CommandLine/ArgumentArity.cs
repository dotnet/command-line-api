// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class ArgumentArity : IArgumentArity
    {
        public const int MaximumArity = 100_000;

        public ArgumentArity(int minimumNumberOfValues, int maximumNumberOfValues)
        {
            if (minimumNumberOfValues < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumNumberOfValues));
            }

            if (maximumNumberOfValues < minimumNumberOfValues)
            {
                throw new ArgumentException($"{nameof(maximumNumberOfValues)} must be greater than or equal to {nameof(minimumNumberOfValues)}");
            }

            if (maximumNumberOfValues > MaximumArity)
            {
                throw new ArgumentException($"{nameof(maximumNumberOfValues)} must be less than or equal to {nameof(MaximumArity)}");
            }

            MinimumNumberOfValues = minimumNumberOfValues;
            MaximumNumberOfValues = maximumNumberOfValues;
        }

        public int MinimumNumberOfValues { get; }

        public int MaximumNumberOfValues { get; }

        internal static FailedArgumentConversionArityResult? Validate(
            SymbolResult symbolResult,
            IArgument argument,
            int minimumNumberOfValues,
            int maximumNumberOfValues)
        {
            var argumentResult = symbolResult switch
            {
                ArgumentResult a => a,
                _ => symbolResult.Root!.FindResultFor(argument)
            };

            var tokenCount = argumentResult?.Tokens.Count ?? 0;

            if (tokenCount < minimumNumberOfValues)
            {
                if (symbolResult!.UseDefaultValueFor(argument))
                {
                    return null;
                }

                return new MissingArgumentConversionResult(
                    argument,
                    symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult));
            }

            if (tokenCount > maximumNumberOfValues)
            {
                return new TooManyArgumentsConversionResult(
                    argument,
                    symbolResult!.ValidationMessages.ExpectsOneArgument(symbolResult));
            }

            return null;
        }

        public static IArgumentArity Zero => new ArgumentArity(0, 0);

        public static IArgumentArity ZeroOrOne => new ArgumentArity(0, 1);

        public static IArgumentArity ExactlyOne => new ArgumentArity(1, 1);

        public static IArgumentArity ZeroOrMore => new ArgumentArity(0, MaximumArity);

        public static IArgumentArity OneOrMore => new ArgumentArity(1, MaximumArity);

        internal static IArgumentArity Default(Type type, Argument argument, ISymbol parent)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string))
            {
                return parent is ICommand
                           ? ZeroOrMore
                           : OneOrMore;
            }

            if (type == typeof(bool))
            {
                return ZeroOrOne;
            }

            if (parent is ICommand &&
                (argument.HasDefaultValue ||
                 type.IsNullable()))
            {
                return ZeroOrOne;
            }

            if (type == typeof(void))
            {
                return Zero;
            }

            return ExactlyOne;
        }
    }
}
