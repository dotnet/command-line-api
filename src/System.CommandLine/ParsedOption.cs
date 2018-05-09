// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ParsedOption : ParsedSymbol
    {
        public ParsedOption(Option option, string token = null, ParsedCommand parent = null) :
            base(option, token ?? option?.ToString(), parent)
        {
        }

        public override ParsedSymbol TryTakeToken(Token token) =>
            TryTakeArgument(token);

        protected internal override ParseError Validate()
        {
            if (arguments.Count > 1 &&
                Symbol.ArgumentsRule.Parser.ArgumentArity != ArgumentArity.Many)
            {
                // TODO: (Validate) localize
                return new ParseError(
                    $"Option '{Symbol}' cannot be specified more than once.",
                    this,
                    false);
            }

            return base.Validate();
        }
    }
}
