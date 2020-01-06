﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Binding
{
    internal class ParseResultMatchingValueSource : IValueSource
    {
        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object boundValue)
        {
            if (!string.IsNullOrEmpty(valueDescriptor.ValueName))
            {
                var commandResult = bindingContext.ParseResult.CommandResult;

                while (commandResult != null)
                {
                    if (commandResult.TryGetValueForOption(valueDescriptor,
                        out var optionValue))
                    {
                        boundValue = optionValue;
                        return true;
                    }

                    if (commandResult.TryGetValueForArgument(
                        valueDescriptor,
                        out var argumentValue))
                    {
                        boundValue = argumentValue;
                        return true;
                    }

                    commandResult = commandResult.Parent as CommandResult;
                }
            }

            boundValue = null;
            return false;
        }
    }
}
