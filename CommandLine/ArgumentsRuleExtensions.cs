// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ArgumentsRuleExtensions
    {
        public static ArgumentsRule And(
            this ArgumentsRule rule,
            Func<AppliedOption, string> error)
        {
            return rule.And(new ArgumentsRule(error));
        }

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
                suggest: result => rules.SelectMany(r => r.Suggest(result)),
                name: rule.Name ?? rule2.Name,
                description: rule.Description ?? rule2.Description,
                defaultValue: rule.GetDefaultValue ?? rule2.GetDefaultValue,
                materialize: rule.Materializer ?? rule2.Materializer);
        }

        public static ArgumentsRule MaterializeAs<T>(
            this ArgumentsRule rule,
            Func<AppliedOption, T> materialize)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (materialize == null)
            {
                throw new ArgumentNullException(nameof(materialize));
            }

            return rule.With(materialize: o => materialize(o));
        }

        public static ArgumentsRule With(
            this ArgumentsRule rule,
            string description = null,
            string name = null,
            Func<string> defaultValue = null,
            Func<AppliedOption, object> materialize = null)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            return new ArgumentsRule(
                validate: rule.Validate,
                allowedValues: rule.AllowedValues,
                defaultValue: defaultValue ??
                              rule.GetDefaultValue,
                name: name ?? rule.Name,
                description: description ?? rule.Description,
                suggest: rule.Suggest,
                materialize: materialize ?? rule.Materialize);
        }
    }
}