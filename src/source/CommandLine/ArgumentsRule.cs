// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentsRule
    {
        private readonly Func<string> defaultValue;

        public static ArgumentsRule None =>
            new ArgumentsRule(new ArgumentParser<string>(o =>
            {
                if (!o.Arguments.Any())
                {
                    return null;
                }

                return ArgumentParseResult.Failure(ValidationMessages.Current.NoArgumentsAllowed(o.Symbol.ToString()));
            }));

        public ArgumentsRule(
            ArgumentParser parser,
            Func<string> defaultValue = null,
            ArgumentsRuleHelp help = null)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            this.defaultValue = defaultValue;

            Help = help ?? new ArgumentsRuleHelp();

            //if (suggest == null)
            //{
            //    this.suggest = (result, position) =>
            //        AllowedValues.FindSuggestions(
            //            result,
            //            position ?? result.ImplicitCursorPosition());
            //}
            //else
            //{
            //    this.suggest = (result, position) =>
            //        suggest(result).ToArray()
            //            .FindSuggestions(
            //                result.TextToMatch(position ?? result.ImplicitCursorPosition()));
            //}
        }

        public Func<string> GetDefaultValue => () => defaultValue?.Invoke();
        
        public bool HasDefaultValue => defaultValue != null;

        public ArgumentsRuleHelp Help { get; }

        public ArgumentParser Parser { get; }
    }
}
