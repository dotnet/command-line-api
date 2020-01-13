// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class ArgumentResult : SymbolResult
    {
        internal ArgumentResult(
            IArgument argument,
            Token token,
            SymbolResult parent) : base(argument, token, parent)
        {
            Argument = argument;
        }

        internal ArgumentConversionResult ConversionResult;

        public IArgument Argument { get; }

        internal override ArgumentConversionResult ArgumentConversionResult =>
            ConversionResult ??= Convert(this, Argument);

        public override string ToString() => $"{GetType().Name} {Argument.Name}: {string.Join(" ", Tokens.Select(t => $"<{t.Value}>"))}";

        internal ParseError CustomError(Argument argument)
        {
            foreach (var symbolValidator in argument.Validators)
            {
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this);
                }
            }

            return null;
        }

        internal static ArgumentConversionResult Convert(
            ArgumentResult argumentResult,
            IArgument argument)
        {
            var parentResult = argumentResult.Parent;

            if (ShouldCheckArity() &&
                ArgumentArity.Validate(parentResult,
                                       argument,
                                       argument.Arity.MinimumNumberOfValues,
                                       argument.Arity.MaximumNumberOfValues) is FailedArgumentConversionResult failedResult)
            {
                return failedResult;
            }

            if (parentResult.UseDefaultValueFor(argument))
            {
                var defaultValueFor = parentResult.GetDefaultValueFor(argument);

                return ArgumentConversionResult.Success(argument, defaultValueFor);
            }

            if (argument is Argument a &&
                a.ConvertArguments != null)
            {
                if (argumentResult.ConversionResult != null)
                {
                    return argumentResult.ConversionResult;
                }

                var success = a.ConvertArguments(argumentResult, out var value);

                if (value is ArgumentConversionResult conversionResult)
                {
                    return conversionResult;
                }
                else if (success)
                {
                    return ArgumentConversionResult.Success(argument, value);
                }
                else 
                {
                    return ArgumentConversionResult.Failure(argument, argumentResult.ErrorMessage ?? $"Invalid: {parentResult.Token} {string.Join(" ", parentResult.Tokens.Select(t => t.Value))}");
                }
            }

            switch (argument.Arity.MaximumNumberOfValues)
            {
                case 0:
                    return ArgumentConversionResult.Success(argument, null);

                case 1:
                    return ArgumentConversionResult.Success(argument, parentResult.Tokens.Select(t => t.Value).SingleOrDefault());

                default:
                    return ArgumentConversionResult.Success(argument, parentResult.Tokens.Select(t => t.Value).ToArray());
            }

            bool ShouldCheckArity()
            {
                return !(parentResult is OptionResult optionResult &&
                       optionResult.IsImplicit);
            }
        }

    }
}
