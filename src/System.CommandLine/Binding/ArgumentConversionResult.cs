// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Binding
{
    internal sealed class ArgumentConversionResult
    {
        internal readonly CliArgumentResultInternal ArgumentResultInternal;
        internal readonly object? Value;
        internal readonly string? ErrorMessage;
        internal ArgumentConversionResultType Result;

        private ArgumentConversionResult(CliArgumentResultInternal argumentResult, string error, ArgumentConversionResultType failure)
        {
            ArgumentResultInternal = argumentResult;
            ErrorMessage = error;
            Result = failure;
        }

        private ArgumentConversionResult(CliArgumentResultInternal argumentResult, object? value, ArgumentConversionResultType result)
        {
            ArgumentResultInternal = argumentResult;
            Value = value;
            Result = result;
        }

        internal static ArgumentConversionResult Failure(CliArgumentResultInternal argumentResult, string error, ArgumentConversionResultType reason)
            => new(argumentResult, error, reason);

        internal static ArgumentConversionResult ArgumentConversionCannotParse(CliArgumentResultInternal argumentResult, Type expectedType, string value)
            => new(argumentResult, FormatErrorMessage(argumentResult, expectedType, value), ArgumentConversionResultType.FailedType);

        public static ArgumentConversionResult Success(CliArgumentResultInternal argumentResult, object? value)
            => new(argumentResult, value, ArgumentConversionResultType.Successful);

        internal static ArgumentConversionResult None(CliArgumentResultInternal argumentResult)
            => new(argumentResult, value: null, ArgumentConversionResultType.NoArgument);

        private static string FormatErrorMessage(
            CliArgumentResultInternal argumentResult,
            Type expectedType,
            string value)
        {
            if (argumentResult.Parent is CliCommandResultInternal commandResult)
            {
                string alias = commandResult.Command.Name;
// TODO: completion
/*
                CompletionItem[] completionItems = argumentResult.Argument.GetCompletions(CompletionContext.Empty).ToArray();

                if (completionItems.Length > 0)
                {
                    return LocalizationResources.ArgumentConversionCannotParseForCommand(
                        value, alias, expectedType, completionItems.Select(ci => ci.Label));
                }
                else
*/
                {
                    return LocalizationResources.ArgumentConversionCannotParseForCommand(value, alias, expectedType);
                }
            }
            else if (argumentResult.Parent is CliOptionResultInternal optionResult)
            {
                string alias = optionResult.Option.Name;
// TODO: completion
/*
                CompletionItem[] completionItems = optionResult.Option.GetCompletions(CompletionContext.Empty).ToArray();

                if (completionItems.Length > 0)
                {
                    return LocalizationResources.ArgumentConversionCannotParseForOption(
                        value, alias, expectedType, completionItems.Select(ci => ci.Label));
                }
                else
*/
                {
                    return LocalizationResources.ArgumentConversionCannotParseForOption(value, alias, expectedType);
                }
            }

            // fake ArgumentResults with no Parent
            return LocalizationResources.ArgumentConversionCannotParse(value, expectedType);
        }
    }
}