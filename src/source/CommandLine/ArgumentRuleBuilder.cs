// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentRuleBuilder
    {
        internal Func<string> DefaultValue { get; set; }

        internal ArgumentsRuleHelp Help { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get;} = new List<ValidateSymbol>();

        public void AddValidator(ValidateSymbol validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            SymbolValidators.Add(validator);
        }

        protected virtual ArgumentParser BuildArgumentParser()
        {
            return new ArgumentParser<ParsedSymbol>(symbol =>
            {
                // TODO: (BuildArgumentParser) this is likely redundant
                foreach (var validator in SymbolValidators)
                {
                    var validationMessage = validator(symbol);

                    if (!string.IsNullOrWhiteSpace(validationMessage))
                    {
                        return ArgumentParseResult.Failure(validationMessage);
                    }
                }

                return ArgumentParseResult.Success(symbol);
            });
        }

        public ArgumentsRule Build()
        {
            return new ArgumentsRule(
                BuildArgumentParser(), 
                DefaultValue, 
                Help, 
                SymbolValidators);
        }

        public static ArgumentRuleBuilder From(ArgumentsRule arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var builder = new ArgumentRuleBuilder
            {
                Help = new ArgumentsRuleHelp(
                    arguments?.Help?.Name,
                    arguments?.Help?.Description)
            };

            foreach (var symbolValidator in arguments.SymbolValidators)
            {
                builder.SymbolValidators.Add(symbolValidator);
            }

            return builder;
        }
    }
}
