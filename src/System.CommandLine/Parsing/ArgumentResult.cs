// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Argument"/>.
    /// </summary>
    public sealed class ArgumentResult : SymbolResult
    {
        private ArgumentConversionResult? _conversionResult;

        internal ArgumentResult(
            Argument argument,
            SymbolResultTree symbolResultTree,
            SymbolResult? parent) : base(symbolResultTree, parent)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to which the result applies.
        /// </summary>
        public Argument Argument { get; }

        internal override int MaximumArgumentCapacity => Argument.Arity.MaximumNumberOfValues;

        internal bool IsImplicit => Argument.HasDefaultValue && Tokens.Count == 0;

        internal IReadOnlyList<Token>? PassedOnTokens { get; private set; }

        internal ArgumentConversionResult GetArgumentConversionResult() =>
            _conversionResult ??= ValidateAndConvert(potentialRecursion: false);

        /// <inheritdoc cref="GetValueOrDefault{T}"/>
        public object? GetValueOrDefault() =>
            GetValueOrDefault<object?>();

        /// <summary>
        /// Gets the parsed value or the default value for <see cref="Argument"/>.
        /// </summary>
        /// <returns>The parsed value or the default value for <see cref="Argument"/></returns>
        public T GetValueOrDefault<T>() =>
            (_conversionResult ??= ValidateAndConvert(potentialRecursion: true))
                .ConvertIfNeeded(typeof(T))
                .GetValueOrDefault<T>();

        /// <summary>
        /// Specifies the maximum number of tokens to consume for the argument. Remaining tokens are passed on and can be consumed by later arguments, or will otherwise be added to <see cref="ParseResult.UnmatchedTokens"/>
        /// </summary>
        /// <param name="numberOfTokens">The number of tokens to take. The rest are passed on.</param>
        /// <exception cref="ArgumentOutOfRangeException">numberOfTokens - Value must be at least 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method is called more than once.</exception>
        public void OnlyTake(int numberOfTokens)
        {
            if (numberOfTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfTokens), numberOfTokens, "Value must be at least 1.");
            }

            if (PassedOnTokens is { })
            {
                throw new InvalidOperationException($"{nameof(OnlyTake)} can only be called once.");
            }

            if (_tokens is not null)
            {
                var passedOnTokensCount = _tokens.Count - numberOfTokens;

                PassedOnTokens = new List<Token>(_tokens.GetRange(numberOfTokens, passedOnTokensCount));

                _tokens.RemoveRange(numberOfTokens, passedOnTokensCount);
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name} {Argument.Name}: {string.Join(" ", Tokens.Select(t => $"<{t.Value}>"))}";

        /// <inheritdoc/>
        public override void AddError(string errorMessage)
        {
            SymbolResultTree.AddError(new ParseError(errorMessage, Parent is OptionResult option ? option : this));
            _conversionResult = ArgumentConversionResult.Failure(this, errorMessage, ArgumentConversionResultType.Failed);
        }

        private ArgumentConversionResult ValidateAndConvert(bool potentialRecursion)
        {
            Debug.Assert(_conversionResult is null);

            if (!ArgumentArity.Validate(this, out ArgumentConversionResult? arityFailure))
            {
                return ReportErrorIfNeeded(arityFailure);
            }

            if (!potentialRecursion && Argument.HasValidators)
            {
                for (var i = 0; i < Argument.Validators.Count; i++)
                {
                    Argument.Validators[i](this);
                }

                // validator provided by the user might report an error, which sets _conversionResult
                if (_conversionResult is not null)
                {
                    return _conversionResult;
                }
            }

            if (Parent!.UseDefaultValueFor(this))
            {
                var defaultValue = Argument.GetDefaultValue(this);

                // default value factory provided by the user might report an error, which sets _conversionResult
                return _conversionResult ?? ArgumentConversionResult.Success(this, defaultValue);
            }

            if (Argument.ConvertArguments is null)
            {
                return Argument.Arity.MaximumNumberOfValues switch
                {
                    1 when _tokens is null => ArgumentConversionResult.None(this),
                    1 when _tokens is not null => ArgumentConversionResult.Success(this, _tokens[0]),
                    _ => ArgumentConversionResult.Success(this, Tokens)
                };
            }

            var success = Argument.ConvertArguments(this, out var value);

            // default value factory provided by the user might report an error, which sets _conversionResult
            if (_conversionResult is not null)
            {
                return _conversionResult;
            }

            if (value is ArgumentConversionResult conversionResult)
            {
                return ReportErrorIfNeeded(conversionResult);
            }

            if (success)
            {
                return ArgumentConversionResult.Success(this, value);
            }

            return ReportErrorIfNeeded(new ArgumentConversionResult(this, Argument.ValueType, Tokens[0].Value));

            ArgumentConversionResult ReportErrorIfNeeded(ArgumentConversionResult result)
            {
                if (result.Result >= ArgumentConversionResultType.Failed)
                {
                    SymbolResultTree.AddError(new ParseError(result.ErrorMessage!, Parent is OptionResult option ? option : this));
                }

                return result;
            }
        }
    }
}
