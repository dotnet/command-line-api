// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class OptionResult : SymbolResult
    {
        internal OptionResult(
            IOption option,
            Token token,
            CommandResult parent = null) :
            base(option ?? throw new ArgumentNullException(nameof(option)),
                 token ?? throw new ArgumentNullException(nameof(token)),
                 parent)
        {
            Option = option;
        }

        public IOption Option { get; }

        public bool IsImplicit => Token is ImplicitToken;

        private protected override int RemainingArgumentCapacity
        {
            get
            {
                var capacity = base.RemainingArgumentCapacity;

                if (IsImplicit && capacity < int.MaxValue)
                {
                    capacity += 1;
                }

                return capacity;
            }
        }
    }
}
