// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class OptionResult : SymbolResult
    {
        public OptionResult(
            IOption option,
            Token token = null,
            CommandResult parent = null) :
            base(option, token ?? option?.DefaultToken(), parent)
        {
            Option = option;
        }

        public IOption Option { get; }

        public bool IsImplicit { get; private set; }

        private protected override int RemainingArgumentCapacity
        {
            get
            {
                var capacity = base.RemainingArgumentCapacity;

                if (IsImplicit && capacity < int.MaxValue
                )
                {
                    capacity += 1;
                }

                return capacity;
            }
        }

        internal override SymbolResult TryTakeToken(Token token) =>
            TryTakeArgument(token);

        internal static OptionResult CreateImplicit(
            IOption option,
            CommandResult parent)
        {
            var result = new OptionResult(option,
                                          option.DefaultToken());

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
                        result.ArgumentResult = ArgumentResult.Success(value);
                        break;
                }
            }

            return result;
        }
    }
}
