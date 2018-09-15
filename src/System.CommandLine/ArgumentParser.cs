// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    internal class ArgumentParser
    {
        public ArgumentParser(
            ArgumentArityValidator arityValidator,
            ConvertArgument convert = null)
        {
            ArityValidator = arityValidator ?? throw new ArgumentNullException(nameof(arityValidator));
            ConvertArguments = convert;
        }

        public ArgumentArityValidator ArityValidator { get; }

        internal ConvertArgument ConvertArguments { get; }

        public ArgumentParseResult Parse(SymbolResult symbolResult)
        {
            var error = ArityValidator.Validate(symbolResult);
            if (!string.IsNullOrWhiteSpace(error))
            {
                return new FailedArgumentArityResult(error);
            }

            if (ConvertArguments != null)
            {
                return ConvertArguments(symbolResult);
            }

            switch (ArityValidator.MaximumNumberOfArguments)
            {
                case 0:
                    return ArgumentParseResult.Success((string)null);

                case 1:
                    return ArgumentParseResult.Success(symbolResult.Arguments.SingleOrDefault());

                default:
                    return ArgumentParseResult.Success(symbolResult.Arguments);
            }
        }
    }
}
