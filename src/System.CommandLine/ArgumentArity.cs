// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics;

namespace System.CommandLine
{
    /// <summary>
    /// Defines the arity of an option or argument.
    /// </summary>
    /// <remarks>The arity refers to the number of values that can be passed on the command line.
    /// </remarks>
    [DebuggerDisplay("\\{{" + nameof(MinimumNumberOfValues) + "},{" + nameof(MaximumNumberOfValues) + "}\\}")]
    public readonly struct ArgumentArity : IEquatable<ArgumentArity>
    {
        private const int MaximumArity = 100_000;

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
            IsNonDefault = true;
        }

        /// <summary>
        /// Gets the minimum number of values required for an <see cref="Argument">argument</see>.
        /// </summary>
        public int MinimumNumberOfValues { get; }

        /// <summary>
        /// Gets the maximum number of values allowed for an <see cref="Argument">argument</see>.
        /// </summary>
        public int MaximumNumberOfValues { get; }

        internal bool IsNonDefault { get;  }

        /// <inheritdoc />
        public bool Equals(ArgumentArity other) => 
            other.MaximumNumberOfValues == MaximumNumberOfValues && 
            other.MinimumNumberOfValues == MinimumNumberOfValues &&
            other.IsNonDefault == IsNonDefault;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ArgumentArity arity && Equals(arity);

        /// <inheritdoc />
        public override int GetHashCode()
            => MaximumNumberOfValues ^ MinimumNumberOfValues ^ IsNonDefault.GetHashCode();

        internal static ArgumentConversionResult? Validate(
            SymbolResult symbolResult,
            Argument argument,
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
                if (symbolResult.UseDefaultValueFor(argument))
                {
                    return null;
                }

                return ArgumentConversionResult.Failure(
                    argument,
                    symbolResult.LocalizationResources.RequiredArgumentMissing(symbolResult, argument),
                    ArgumentConversionResultType.FailedMissingArgument);
            }

            if (tokenCount > maximumNumberOfValues)
            {
                if (symbolResult is OptionResult optionResult)
                {
                    if (!optionResult.Option.AllowMultipleArgumentsPerToken)
                    {
                        return ArgumentConversionResult.Failure(
                            argument,
                            symbolResult!.LocalizationResources.ExpectsOneArgument(symbolResult),
                            ArgumentConversionResultType.FailedTooManyArguments);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// An arity that does not allow any values.
        /// </summary>
        public static ArgumentArity Zero => new(0, 0);

        /// <summary>
        /// An arity that may have one value, but no more than one.
        /// </summary>
        public static ArgumentArity ZeroOrOne => new(0, 1);

        /// <summary>
        /// An arity that must have exactly one value.
        /// </summary>
        public static ArgumentArity ExactlyOne => new(1, 1);

        /// <summary>
        /// An arity that may have multiple values.
        /// </summary>
        public static ArgumentArity ZeroOrMore => new(0, MaximumArity);

        /// <summary>
        /// An arity that must have at least one value.
        /// </summary>
        public static ArgumentArity OneOrMore => new(1, MaximumArity);

        internal static ArgumentArity Default(Type type, Argument argument, ParentNode? firstParent)
        {
            if (type == typeof(bool) || type == typeof(bool?))
            {
                return ZeroOrOne;
            }

            var parent = firstParent?.Symbol;

            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                return parent is Command
                           ? ZeroOrMore
                           : OneOrMore;
            }

            if (parent is Command &&
                (argument.HasDefaultValue ||
                 type.IsNullable()))
            {
                return ZeroOrOne;
            }

            return ExactlyOne;
        }
    }
}
