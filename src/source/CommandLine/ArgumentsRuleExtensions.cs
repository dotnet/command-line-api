// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ArgumentsRuleExtensions
    {
        public static ArgumentsRule And(
            this ArgumentsRule rule,
            ArgumentsRule rule2)
        {
            var rules = new[] { rule, rule2 };

            return new ArgumentsRule(
                validate: option => rules.Select(r => r.Validate(option))
                                         .FirstOrDefault(result => !string.IsNullOrWhiteSpace(result)),
                allowedValues: rules.SelectMany(r => r.AllowedValues)
                                    .Distinct()
                                    .ToArray(),
                suggest: (result, position) => rules.SelectMany(r => r.Suggest(result, position)),
                help: new ArgumentsRuleHelp(name: rule.Help.Name ?? rule2.Help.Name, 
                                            description: rule.Help.Description ?? rule2.Help.Description),
                defaultValue: (rule.HasDefaultValue
                                   ? rule.GetDefaultValue
                                   : null)
                              ??
                              (rule2.HasDefaultValue
                                   ? rule2.GetDefaultValue
                                   : null),
                materialize: rule.Materializer ?? rule2.Materializer);
        }
    }
}