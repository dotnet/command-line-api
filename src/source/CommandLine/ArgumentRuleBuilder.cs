// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentRuleBuilder
    {
        private readonly List<Validate<string>> validators = new List<Validate<string>>();

        public void AddValidator(Validate<string> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            validators.Add(validator);
        }

        public ArgumentsRuleHelp Help { get; set; }

        public Func<string> DefaultValue { get; set; }

        public Convert Convert { get; }

        protected virtual ArgumentParser BuildArgumentParser()
        {
            var parser = new ArgumentParser<string>(Convert ?? (symbol => ArgumentParseResult.Success(symbol.Token)));
            foreach (Validate<string> validator in validators)
            {
                parser.AddValidator(validator);
            }
            return parser;
        }

        public ArgumentsRule Build()
        {
            return new ArgumentsRule(BuildArgumentParser(), DefaultValue, Help);
        }
    }
}
