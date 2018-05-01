// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentParser<T> : ArgumentParser
    {
        private readonly List<ValidateArgument<T>> argumentValidators = new List<ValidateArgument<T>>();

        private readonly ConvertArgument convertArgument;

        public ArgumentParser(ConvertArgument convertArgument)
        {
            this.convertArgument = convertArgument ?? 
                                   throw new ArgumentNullException(nameof(convertArgument));
        }

        public void AddValidator(ValidateArgument<T> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            argumentValidators.Add(validator);
        }

        //string -> parsed symbol -> type conversion -> (type checking) -> validation

        public override ArgumentParseResult Parse(ParsedSymbol symbol)
        {
            var convertResult = convertArgument(symbol);

            if (convertResult is SuccessfulArgumentParseResult<T> successfulResult)
            {
                return ValidateConvertedArgument(successfulResult.Value, symbol);
            }

            return convertResult;
        }

        private ArgumentParseResult ValidateConvertedArgument(
            T value, 
            ParsedSymbol symbol)
        {
            ArgumentParseResult result = null;

            foreach (var validator in argumentValidators)
            {
                result = validator(value, symbol);

                if (result is SuccessfulArgumentParseResult<T> successResult)
                {
                    // TODO: (ValidateArgument) can this not overwrite the previous value if there's more than one validator?
                    value = successResult.Value;
                }
                else
                {
                    return result;
                }
            }

            return result ?? ArgumentParseResult.Success(value);
        }
    }
}
