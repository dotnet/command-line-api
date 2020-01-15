// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public class ArgumentArity : IArgumentArity
    {
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

            MinimumNumberOfValues = minimumNumberOfValues;
            MaximumNumberOfValues = maximumNumberOfValues;
        }

        public int MinimumNumberOfValues { get; set; }

        public int MaximumNumberOfValues { get; set; }

        internal static FailedArgumentConversionArityResult Validate(
            ArgumentResult argumentResult) =>
            Validate(argumentResult.Parent,
                     argumentResult.Argument,
                     argumentResult.Argument.Arity.MinimumNumberOfValues,
                     argumentResult.Argument.Arity.MaximumNumberOfValues);

        internal static FailedArgumentConversionArityResult Validate(
            SymbolResult symbolResult,
            IArgument argument,
            int minimumNumberOfValues,
            int maximumNumberOfValues)
        {
            SymbolResult argumentResult = null;

            switch (symbolResult)
            {
                case CommandResult commandResult:
                    argumentResult = commandResult.Root.FindResultFor(argument);
                    break;

                case OptionResult optionResult:
                    argumentResult = optionResult.Children.ResultFor(argument);
                    break;

                case ArgumentResult _:
                    throw new ArgumentException("");
            }

            var tokenCount = argumentResult?.Tokens.Count ?? 0;

            if (tokenCount < minimumNumberOfValues)
            {
                if (symbolResult.UseDefaultValueFor(argument))
                {
                    return null;
                }

                return new MissingArgumentConversionResult(
                    argument,
                    symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult));
            }

            if (tokenCount > maximumNumberOfValues)
            {
                if (maximumNumberOfValues == 1)
                {
                    return new TooManyArgumentsConversionResult(
                        argument,
                        symbolResult.ValidationMessages.ExpectsOneArgument(symbolResult));
                }
                else
                {
                    return new TooManyArgumentsConversionResult(
                        argument,
                        symbolResult.ValidationMessages.ExpectsFewerArguments(
                            symbolResult.Tokens.Last(),
                            tokenCount,
                            maximumNumberOfValues));
                }
            }

            return null;
        }

        public static IArgumentArity Zero => new ArgumentArity(0, 0);

        public static IArgumentArity ZeroOrOne => new ArgumentArity(0, 1);

        public static IArgumentArity ExactlyOne => new ArgumentArity(1, 1);

        public static IArgumentArity ZeroOrMore => new ArgumentArity(0, byte.MaxValue);

        public static IArgumentArity OneOrMore => new ArgumentArity(1, byte.MaxValue);

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

            return ExactlyOne;
        }
    }
}
