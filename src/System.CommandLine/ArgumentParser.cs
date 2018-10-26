// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    internal class ArgumentParser
    {
        public ArgumentParser(
            ArgumentArity arity,
            ConvertArgument convert = null)
        {
            Arity = arity ?? throw new ArgumentNullException(nameof(arity));
            ConvertArguments = convert;
        }

        public ArgumentArity Arity { get; }

        internal ConvertArgument ConvertArguments { get; }

        public ArgumentParseResult Parse(SymbolResult symbolResult)
        {
            var error = Arity.Validate(symbolResult);

            if (error != null)
            {
                return error;
            }
            
            if (ConvertArguments != null)
            {
                return ConvertArguments(symbolResult);
            }

            switch (Arity.MaximumNumberOfArguments)
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
