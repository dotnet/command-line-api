// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentsRule
    {
        private readonly Func<string> defaultValue;

        public ArgumentsRule(
            ArgumentParser parser = null,
            Func<string> defaultValue = null,
            ArgumentsRuleHelp help = null,
            IReadOnlyCollection<ValidateSymbol> symbolValidators = null)
        {
            Parser = parser;

            this.defaultValue = defaultValue;

            Help = help ?? new ArgumentsRuleHelp();

            if (symbolValidators != null)
            {
                SymbolValidators.AddRange(symbolValidators);
            }

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

        internal List<ValidateSymbol> SymbolValidators { get; } = new List<ValidateSymbol>();

        public static ArgumentsRule None { get; } = new ArgumentsRule(symbolValidators: new ValidateSymbol[] { AcceptNoArguments });

        private static string AcceptNoArguments(ParsedSymbol o)
        {
            if (!o.Arguments.Any())
            {
                return null;
            }

            return ValidationMessages.Current.NoArgumentsAllowed(o.Symbol.ToString());
        }
    }
}
