// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the arity of an argument.
    /// </summary>
    public class ArgumentArity : IArgumentArity
    {
        /// <summary>
        /// The overall maximum number of values that can be provided to any argument.
        /// </summary>
        public const int MaximumArity = 100_000;

        /// <summary>
        /// Initializes a new instance of the ArgumentArity class.
        /// </summary>
        /// <param name="minimumNumberOfValues">The minimum number of values required for the argument.</param>
        /// <param name="maximumNumberOfValues">The maximum number of values allowed for the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimumNumberOfValues"/> is negative.</exception>
        /// <exception cref="ArgumentException">Thrown when the maximum number is less than the minimum number or the maximum number is greater than MaximumArity.</exception>
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

        /// <summary>
        /// Gets the minimum number of values required for the argument.
        /// </summary>
        public int MinimumNumberOfValues { get; }

        /// <summary>
        /// Gets the maximum number of values allowed for the argument.
        /// </summary>
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

        /// <summary>
        /// Must not have any arguments.
        /// </summary>
        public static IArgumentArity Zero => new ArgumentArity(0, 0);

        /// <summary>
        /// May have one argument, but no more than one.
        /// </summary>
        public static IArgumentArity ZeroOrOne => new ArgumentArity(0, 1);

        /// <summary>
        /// Must have exactly one argument.
        /// </summary>
        public static IArgumentArity ExactlyOne => new ArgumentArity(1, 1);

        /// <summary>
        /// May have multiple arguments.
        /// </summary>
        public static IArgumentArity ZeroOrMore => new ArgumentArity(0, MaximumArity);

        /// <summary>
        /// Must have at least one argument.
        /// </summary>
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
