// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ArgumentArity 
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

        internal FailedArgumentArityResult Validate(SymbolResult symbolResult)
        {
            if (symbolResult.Arguments.Count < MinimumNumberOfArguments)
            {
                return new FailedArgumentArityResult(symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult));
            }

            if (symbolResult.Arguments.Count > MaximumNumberOfArguments)
            {
                if (MaximumNumberOfArguments == 1)
                {
                    return new FailedArgumentArityResult(symbolResult.ValidationMessages.ExpectsOneArgument(symbolResult));
                }
                else
                {
                    return new FailedArgumentArityResult(symbolResult.ValidationMessages.ExpectsFewerArguments(symbolResult, MaximumNumberOfArguments));
                }
            }

            return null;
        }

        public static ArgumentArity Zero { get; } = new ArgumentArity(0, 0);

        public static ArgumentArity ZeroOrOne { get; } = new ArgumentArity(0, 1);

        public static ArgumentArity ExactlyOne { get; } = new ArgumentArity(1, 1);

        public static ArgumentArity ZeroOrMore { get; } = new ArgumentArity(0, int.MaxValue);

        public static ArgumentArity OneOrMore { get; } = new ArgumentArity(1, int.MaxValue);
    }
}
