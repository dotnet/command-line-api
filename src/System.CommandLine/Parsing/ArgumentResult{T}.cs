// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Parsing
{
    public class ArgumentResult<T> : ArgumentResult
    {
        private T _value;

        internal ArgumentResult(
            Argument<T> argument,
            SymbolResult parent) : base(argument, parent)
        {
        }

        public T Value
        {
            get => ArgumentConversionResult.GetValueOrDefault<T>();
            set
            {
                _value = value;
                ValueWasSpecified = true;
            }
        }

        internal bool ValueWasSpecified { get; set; }

        internal override ArgumentConversionResult Convert(IArgument argument)
        {
            if (ValueWasSpecified)
            {
                return new SuccessfulArgumentConversionResult(argument, _value);
            }
            else
            {
                return base.Convert(argument);
            }
        }
    }
}