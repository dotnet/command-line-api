// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ArgumentArityValidator : ISymbolValidator
    {
        public ArgumentArityValidator(int minimumNumberOfArguments, int maximumNumberOfArguments)
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

        public int MinimumNumberOfArguments { get; }

        public int MaximumNumberOfArguments { get; }

        public string Validate(SymbolResult symbolResult)
        {
            if (symbolResult.Arguments.Count < MinimumNumberOfArguments)
            {
                return symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult);
            }

            if (symbolResult.Arguments.Count > MaximumNumberOfArguments)
            {
                if (MaximumNumberOfArguments == 1)
                {
                    return symbolResult.ValidationMessages.ExpectsOneArgument(symbolResult);
                }
                else
                {
                    return symbolResult.ValidationMessages.ExpectsFewerArguments(symbolResult, MaximumNumberOfArguments);
                }
            }

            return null;
        }
    }
}
