// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class OptionResult : SymbolResult
    {
        public OptionResult(
            IOption option,
            string token = null,
            CommandResult parent = null) :
            base(option, token ?? option?.Token(), parent)
        {
            Option = option;
        }

        public IOption Option { get; }

        internal bool IsImplicit { get; private set; }

        internal override SymbolResult TryTakeToken(Token token) =>
            TryTakeArgument(token);

        protected internal override ParseError Validate()
        {
            if (Arguments.Count > 1 &&
                Symbol.Argument.ArgumentArity.MaximumNumberOfArguments == 1)
            {
                // TODO: (Validate) localize
                return new ParseError(
                    $"Option '{Symbol.Token()}' cannot be specified more than once.",
                    this,
                    false);
            }

            return base.Validate();
        }

        internal static OptionResult CreateImplicit(
            IOption option,
            CommandResult parent)
        {
            var result = new OptionResult(option,
                                          option.Token());

            result.IsImplicit = true;

            if (option.Argument.HasDefaultValue)
            {
                var value = option.Argument.GetDefaultValue();

                switch (value)
                {
                    case string arg:
                        result.TryTakeToken(new Token(arg, TokenType.Argument));
                        break;

                    default:
                        result.Result = ArgumentParseResult.Success(value);
                        break;
                }
            }

            return result;
        }
    }
}
