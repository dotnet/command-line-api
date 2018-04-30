// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentRuleBuilder
    {
        private readonly List<Validate> validators = new List<Validate>();

        public void AddValidator(Validate validator)
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
            => new ArgumentParser<string>(Convert ?? (symbol => ArgumentParseResult.Success(symbol.Token)));

        public ArgumentsRule Build()
        {
            return new ArgumentsRule(BuildArgumentParser(), DefaultValue, Help);
        }
    }
}
