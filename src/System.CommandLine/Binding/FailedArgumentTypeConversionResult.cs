﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Binding
{
    internal class FailedArgumentTypeConversionResult : FailedArgumentConversionResult
    {
        internal FailedArgumentTypeConversionResult(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources) :
            base(argument, FormatErrorMessage(argument, expectedType, value, localizationResources))
        {
        }

        private static string FormatErrorMessage(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources)
        {
            if (argument is Argument a &&
                a.FirstParent is not null && a.FirstParent.Next is null)
            {
                var firstParent = (IdentifierSymbol) a.FirstParent.Symbol;
                var alias = firstParent.Aliases.First();
                
                switch(firstParent)
                {
                    case Command _:
                        return localizationResources.ArgumentConversionCannotParseForCommand(value, alias, expectedType);
                    case Option _:
                        return localizationResources.ArgumentConversionCannotParseForOption(value, alias, expectedType);
                }
            }

            return localizationResources.ArgumentConversionCannotParse(value, expectedType);
        }
    }
}
