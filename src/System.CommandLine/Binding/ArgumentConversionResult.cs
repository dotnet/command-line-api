// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Binding
{
    internal sealed class ArgumentConversionResult
    {
        internal readonly Argument Argument;
        internal readonly object? Value;
        internal readonly string? ErrorMessage;
        internal ArgumentConversionResultType Result;

        private ArgumentConversionResult(Argument argument, string error, ArgumentConversionResultType failure)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
            ErrorMessage = error ?? throw new ArgumentNullException(nameof(error));
            Result = failure;
        }

        private ArgumentConversionResult(Argument argument, object? value)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
            Value = value;
            Result = ArgumentConversionResultType.Successful;
        }

        private ArgumentConversionResult(Argument argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
            Result = ArgumentConversionResultType.NoArgument;
        }

        internal ArgumentConversionResult(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources) :
            this(argument, FormatErrorMessage(argument, expectedType, value, localizationResources), ArgumentConversionResultType.FailedType)
        {
        }

        internal static ArgumentConversionResult Failure(Argument argument, string error, ArgumentConversionResultType reason) => new(argument, error, reason);

        public static ArgumentConversionResult Success(Argument argument, object? value) => new(argument, value);

        internal static ArgumentConversionResult None(Argument argument) => new(argument);

        private static string FormatErrorMessage(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources)
        {
            if (argument.FirstParent?.Symbol is IdentifierSymbol identifierSymbol &&
                argument.FirstParent.Next is null)
            {
                var alias = identifierSymbol.Aliases.First();

                switch (identifierSymbol)
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