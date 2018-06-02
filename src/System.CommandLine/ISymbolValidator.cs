// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface ISymbolValidator
    {

    }

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
    }
}
