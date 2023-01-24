// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Binding
{
    internal sealed class ArgumentConversionResult
    {
        internal readonly ArgumentResult ArgumentResult;
        internal readonly object? Value;
        internal readonly string? ErrorMessage;
        internal ArgumentConversionResultType Result;

        private ArgumentConversionResult(ArgumentResult argumentResult, string error, ArgumentConversionResultType failure)
        {
            ArgumentResult = argumentResult ?? throw new ArgumentNullException(nameof(argumentResult));
            ErrorMessage = error ?? throw new ArgumentNullException(nameof(error));
            Result = failure;
        }

        private ArgumentConversionResult(ArgumentResult argumentResult, object? value)
        {
            ArgumentResult = argumentResult ?? throw new ArgumentNullException(nameof(argumentResult));
            Value = value;
            Result = ArgumentConversionResultType.Successful;
        }

        private ArgumentConversionResult(ArgumentResult argumentResult)
        {
            ArgumentResult = argumentResult ?? throw new ArgumentNullException(nameof(argumentResult));
            Result = ArgumentConversionResultType.NoArgument;
        }

        internal ArgumentConversionResult(
            ArgumentResult argumentResult,
            Type expectedType,
            string value) :
            this(argumentResult, FormatErrorMessage(argumentResult, expectedType, value), ArgumentConversionResultType.FailedType)
        {
        }

        internal static ArgumentConversionResult Failure(ArgumentResult argumentResult, string error, ArgumentConversionResultType reason)
            => new(argumentResult, error, reason);

        public static ArgumentConversionResult Success(ArgumentResult argumentResult, object? value)
            => new(argumentResult, value);

        internal static ArgumentConversionResult None(ArgumentResult argumentResult)
            => new(argumentResult);

        private static string FormatErrorMessage(
            ArgumentResult argumentResult,
            Type expectedType,
            string value)
        {
            if (argumentResult.Parent is CommandResult commandResult)
            {
                string alias = commandResult.Command.GetLongestAlias(removePrefix: false);
                CompletionItem[] completionItems = argumentResult.Argument.GetCompletions(CompletionContext.Empty).ToArray();

                if (completionItems.Length > 0)
                {
                    return argumentResult.LocalizationResources.ArgumentConversionCannotParseForCommand(
                        value, alias, expectedType, completionItems.Select(ci => ci.Label));
                }
                else
                {
                    return argumentResult.LocalizationResources.ArgumentConversionCannotParseForCommand(value, alias, expectedType);
                }
            }
            else if (argumentResult.Parent is OptionResult optionResult)
            {
                string alias = optionResult.Option.GetLongestAlias(removePrefix: false);
                CompletionItem[] completionItems = optionResult.Option.GetCompletions(CompletionContext.Empty).ToArray();

                if (completionItems.Length > 0)
                {
                    return argumentResult.LocalizationResources.ArgumentConversionCannotParseForOption(
                        value, alias, expectedType, completionItems.Select(ci => ci.Label));
                }
                else
                {
                    return argumentResult.LocalizationResources.ArgumentConversionCannotParseForOption(value, alias, expectedType);
                }
            }

            // fake ArgumentResults with no Parent
            return argumentResult.LocalizationResources.ArgumentConversionCannotParse(value, expectedType);
        }
    }
}