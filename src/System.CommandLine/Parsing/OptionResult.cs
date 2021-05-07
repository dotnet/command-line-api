// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class OptionResult : SymbolResult
    {
        private ArgumentConversionResult? _argumentConversionResult;

        internal OptionResult(
            IOption option,
            Token? token = null,
            CommandResult? parent = null) :
            base(option ?? throw new ArgumentNullException(nameof(option)),
                 parent)
        {
            Option = option;
            Token = token;
        }

        public IOption Option { get; }

        public bool IsImplicit => Token is ImplicitToken || Token is null;

        public Token? Token { get; }

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
                if (_argumentConversionResult is null)
                {
                    for (var i = 0; i < Children.Count; i++)
                    {
                        var child = Children[i];

                        if (child is ArgumentResult argumentResult)
                        {
                            return _argumentConversionResult = argumentResult.GetArgumentConversionResult();
                        }
                    }

                    return _argumentConversionResult = ArgumentConversionResult.None(Option.Argument);
                }

                return _argumentConversionResult;
            }
        }

        internal override bool UseDefaultValueFor(IArgument argument) => IsImplicit;
    }
}
