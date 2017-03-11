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
            params ArgumentsRule[] rules)
        {
            rules = new[] { rule }.Concat(rules).ToArray();

            return new ArgumentsRule(
                validate: option => rules.Select(r => r.Validate(option))
                                         .FirstOrDefault(result => !String.IsNullOrWhiteSpace(result)),
                allowedValues: rules.SelectMany(r => r.AllowedValues).Distinct().ToArray(),
                suggest: result => rules.SelectMany(r => r.Suggest(result)),
                name: rule.Name,
                description: rule.Description,
                defaultValue: rule.GetDefaultValue,
                materialize: rule.Materialize);
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
                              (() => rule.DefaultValue),
                name: name ?? rule.Name,
                description: description ?? rule.Description,
                suggest: rule.Suggest,
                materialize: materialize ?? rule.Materialize);
        }
    }
}