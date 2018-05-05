// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using static System.CommandLine.ArgumentArity;

namespace System.CommandLine
{
    public class ArgumentParser
    {
        public ArgumentParser(
            ArgumentArity argumentArity,
            ConvertArgument convert = null)
        {
            ArgumentArity = argumentArity;
            ConvertArguments = convert;
        }

        public ArgumentArity ArgumentArity { get; }

        internal ConvertArgument ConvertArguments { get; }

        public ArgumentParseResult Parse(ParsedSymbol symbol)
        {
            if (ConvertArguments != null)
            {
                return ConvertArguments(symbol);
            }

            switch (ArgumentArity)
            {
                case Zero:
                    return ArgumentParseResult.Success((string) null);
                case One:
                    return ArgumentParseResult.Success(symbol.Arguments.Single());
                case Many:
                default:
                    return ArgumentParseResult.Success(symbol.Arguments);
            }
        }
    }
}
