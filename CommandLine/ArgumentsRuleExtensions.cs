using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ArgumentsRuleExtensions
    {
        public static ArgumentsRule With(
            this ArgumentsRule rule,
            string description = null,
            string name = null,
            Func<string> defaultValue = null)
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
                description: name ?? rule.Name,
                name: description ?? rule.Description);
        }
    }
}