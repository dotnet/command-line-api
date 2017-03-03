// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Create
    {
        internal static ArgumentsRule ParseRule(
            Func<AppliedOption, string> validate,
            IReadOnlyCollection<string> values = null) =>
            new ArgumentsRule(validate, values);

        internal static ArgumentsRule And(
            this ArgumentsRule rule,
            params ArgumentsRule[] rules)
        {
            rules = new[] { rule }.Concat(rules).ToArray();

            return new ArgumentsRule(
                validate:   option => rules.Select(r => r.Validate(option))
                               .FirstOrDefault(result => !string.IsNullOrWhiteSpace(result)),
                allowedValues:  rules.SelectMany(r => r.AllowedValues).Distinct().ToArray(),
                suggest: result => rules.SelectMany(r => r.Suggest(result)), 
                name: rule.Name,
                description: rule.Description,
                defaultValue: rule.GetDefaultValue);
        }

        public static Option Option(
            string aliases,
            string help,
            ArgumentsRule arguments = null,
            Func<AppliedOption, object> materialize = null) =>
            new Option(aliases.Split('|'), help, arguments, materialize: materialize);

        public static Command Command(
            string name,
            string help,
            ArgumentsRule arguments = null,
            Option[] options = null,
            Func<AppliedOption, object> materialize = null) =>
            new Command(
                name,
                help,
                options,
                arguments,
                materialize);

        public static Command Command(
            string name,
            string help) =>
            new Command(name, help);

        public static Command Command(
            string name,
            string help,
            params Option[] options) =>
            new Command(name, help, options);

        public static Command Command(
            string name,
            string help,
            ArgumentsRule arguments,
            params Option[] options) =>
            new Command(name, help, options, arguments);

        public static Command Command(
            string name,
            string help,
            params Command[] commands) =>
            new Command(name, help, commands);
    }
}