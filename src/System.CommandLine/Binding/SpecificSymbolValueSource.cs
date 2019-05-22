// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class OptionValueSource : IValueSource
    {
        public OptionValueSource(IOption option)
        {
            Option = option;
        }

        public IOption Option { get; }

        public bool TryGetValue(
            IValueDescriptor valueDescriptor, 
            BindingContext bindingContext, 
            out object boundValue)
        {
            var result = bindingContext.ParseResult.FindResultFor(Option);

            switch (result)
            {
                case OptionResult optionResult:
                    boundValue = optionResult.GetValueOrDefault();
                    return true;

                default:
                    boundValue = Option.GetDefaultValue();
                    return true;
            }
        }
    }
}
