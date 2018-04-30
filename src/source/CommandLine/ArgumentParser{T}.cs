// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentParser<T> : ArgumentParser
    {
        private readonly List<Validate<T>> validations = new List<Validate<T>>();
        private readonly Convert convert;

        public ArgumentParser()
        {

        }

        public ArgumentParser(Convert convert)
        {
            this.convert = convert ??
                           throw new ArgumentNullException(nameof(convert));
        }

        public void AddValidator(Validate<T> validator)
        {
            validations.Add(validator);
        }

        private ArgumentParseResult Validate(T value, ParsedSymbol symbol)
        {
            ArgumentParseResult result = null;
            foreach (Validate<T> validator in validations)
            {
                result = validator(value, symbol);
                if (result is SuccessfulArgumentParseResult<T> successResult)
                {
                    value = successResult.Value;
                }
                else
                {
                    return result;
                }
            }
            return result ?? ArgumentParseResult.Success(value);
        }

        //string -> parsed symbol -> type conversion -> (type checking) -> validation

        public override ArgumentParseResult Parse(ParsedSymbol symbol)
        {
            ArgumentParseResult typeResult = convert(symbol);
            if (typeResult is SuccessfulArgumentParseResult<T> successfulResult)
            {
                return Validate(successfulResult.Value, symbol);
            }
            return typeResult;
        }
    }
}