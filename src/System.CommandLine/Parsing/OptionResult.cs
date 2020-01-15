// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class OptionResult : SymbolResult
    {
        private ArgumentConversionResult _argumentConversionResult;

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

        internal ArgumentConversionResult ArgumentConversionResult
        {
            get
            {
                if (_argumentConversionResult == null)
                {
                    var results = Children
                                  .OfType<ArgumentResult>()
                                  .Select(r => r.Convert(r.Argument));

                    _argumentConversionResult = results.SingleOrDefault() ??
                                                ArgumentConversionResult.None(Option.Argument);
                }

                return _argumentConversionResult;
            }
        }
    }
}
