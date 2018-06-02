// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class Option : Symbol
    {
        public Option(OptionDefinition optionDefinition, string token = null, Command parent = null) :
            base(optionDefinition, token ?? optionDefinition?.ToString(), parent)
        {
            Definition = optionDefinition;
        }

        public OptionDefinition Definition { get; }

        internal override Symbol TryTakeToken(Token token) =>
            TryTakeArgument(token);

        protected internal override ParseError Validate()
        {
            if (Arguments.Count > 1 &&
                SymbolDefinition.ArgumentDefinition.Parser.ArityValidator.MaximumNumberOfArguments == 1)
            {
                // TODO: (Validate) localize
                return new ParseError(
                    $"Option '{SymbolDefinition.Token()}' cannot be specified more than once.",
                    this,
                    false);
            }

            return base.Validate();
        }
    }
}
