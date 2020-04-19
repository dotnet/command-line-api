﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class ArgumentResult : SymbolResult
    {
        private ArgumentConversionResult _conversionResult;

        internal ArgumentResult(
            IArgument argument,
            SymbolResult parent) : base(argument, parent)
        {
            Argument = argument;
        }

        public IArgument Argument { get; }

        internal ArgumentConversionResult GetArgumentConversionResult() => 
            _conversionResult ??= Convert(Argument);

        public override string ToString() => $"{GetType().Name} {Argument.Name}: {string.Join(" ", Tokens.Select(t => $"<{t.Value}>"))}";

        internal ParseError CustomError(Argument argument)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return new ParseError(ErrorMessage, this);
            }

            for (var i = 0; i < argument.Validators.Count; i++)
            {
                var symbolValidator = argument.Validators[i];
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this);
                }
            }

            return null;
        }

        internal virtual ArgumentConversionResult Convert(
            IArgument argument)
        {
            var parentResult = Parent;

            if (ShouldCheckArity() &&
                ArgumentArity.Validate(parentResult,
                                       argument,
                                       argument.Arity.MinimumNumberOfValues,
                                       argument.Arity.MaximumNumberOfValues) is FailedArgumentConversionResult failedResult)
            {
                return failedResult;
            }

            if (argument is Argument arg)
            {
                if (parentResult.UseDefaultValueFor(argument))
                {
                    var argumentResult = new ArgumentResult(arg, Parent);

                    var defaultValue = arg.GetDefaultValue(argumentResult);

                    if (string.IsNullOrEmpty(argumentResult.ErrorMessage))
                    {
                        return ArgumentConversionResult.Success(
                            argument,
                            defaultValue);
                    }
                    else
                    {
                        return ArgumentConversionResult.Failure(
                            argument,
                            argumentResult.ErrorMessage);
                    }
                }

                if (arg.ConvertArguments != null)
                {
                    if (_conversionResult != null)
                    {
                        return _conversionResult;
                    }

                    var success = arg.ConvertArguments(this, out var value);

                    if (value is ArgumentConversionResult conversionResult)
                    {
                        return conversionResult;
                    }

                    if (success)
                    {
                        return ArgumentConversionResult.Success(argument, value);
                    }

                    return ArgumentConversionResult.Failure(
                        argument,
                        ErrorMessage ?? $"Invalid: {parentResult.Token()} {string.Join(" ", Tokens.Select(t => t.Value))}");
                }
            }

            switch (argument.Arity.MaximumNumberOfValues)
            {
                case 1:
                    return ArgumentConversionResult.Success(
                        argument,
                        Tokens.Select(t => t.Value).SingleOrDefault());

                default:
                    return ArgumentConversionResult.Success(
                        argument,
                        Tokens.Select(t => t.Value).ToArray());
            }

            bool ShouldCheckArity() =>
                !(parentResult is OptionResult optionResult &&
                  optionResult.IsImplicit);
        }
    }
}
