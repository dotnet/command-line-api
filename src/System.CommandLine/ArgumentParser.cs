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
            ArityValidator = arityValidator;
            ConvertArguments = convert;
        }

        public ArgumentArityValidator ArityValidator { get; }

        internal ConvertArgument ConvertArguments { get; }

        public ArgumentParseResult Parse(Symbol symbol)
        {
            if (ConvertArguments != null)
            {
                return ConvertArguments(symbol);
            }

            switch (ArityValidator?.MaximumNumberOfArguments)
            {
                case 0:
                    return ArgumentParseResult.Success((string) null);
                case 1:
                    return ArgumentParseResult.Success(symbol.Arguments.SingleOrDefault());
                default:
                    return ArgumentParseResult.Success(symbol.Arguments);
            }
        }
    }
}
