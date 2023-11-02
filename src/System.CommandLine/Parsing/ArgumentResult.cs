// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Argument"/>.
    /// </summary>
    public sealed class ArgumentResult : SymbolResult
    {
        private ArgumentConversionResult? _conversionResult;
        private bool _onlyTakeHasBeenCalled;

        internal ArgumentResult(
            CliArgument argument,
            SymbolResultTree symbolResultTree,
            SymbolResult? parent) : base(symbolResultTree, parent)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to which the result applies.
        /// </summary>
        public CliArgument Argument { get; }

        internal bool ArgumentLimitReached => Argument.Arity.MaximumNumberOfValues == (_tokens?.Count ?? 0);

        internal ArgumentConversionResult GetArgumentConversionResult() =>
            _conversionResult ??= ValidateAndConvert(useValidators: true);

        /// <summary>
        /// Gets the parsed value or the default value for <see cref="Argument"/>.
        /// </summary>
        /// <returns>The parsed value or the default value for <see cref="Argument"/></returns>
        public T GetValueOrDefault<T>() =>
            (_conversionResult ??= ValidateAndConvert(useValidators: false))
                .ConvertIfNeeded(typeof(T))
                .GetValueOrDefault<T>();

        /// <summary>
        /// Specifies the maximum number of tokens to consume for the argument. Remaining tokens are passed on and can be consumed by later arguments, or will otherwise be added to <see cref="ParseResult.UnmatchedTokens"/>
        /// </summary>
        /// <param name="numberOfTokens">The number of tokens to take. The rest are passed on.</param>
        /// <exception cref="ArgumentOutOfRangeException">numberOfTokens - Value must be at least 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method is called more than once.</exception>
        /// <exception cref="NotSupportedException">Thrown if this method is called by Option-owned ArgumentResult.</exception>
        public void OnlyTake(int numberOfTokens)
        {
            if (numberOfTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfTokens), numberOfTokens, "Value must be at least 1.");
            }

            if (_onlyTakeHasBeenCalled)
            {
                throw new InvalidOperationException($"{nameof(OnlyTake)} can only be called once.");
            }

            if (Parent is OptionResult)
            {
                throw new NotSupportedException($"{nameof(OnlyTake)} is supported only for a {nameof(CliCommand)}-owned {nameof(ArgumentResult)}");
            }

            _onlyTakeHasBeenCalled = true;

            if (_tokens is null || numberOfTokens >= _tokens.Count)
            {
                return;
            }

            CommandResult parent = (CommandResult)Parent!;
            var arguments = parent.Command.Arguments;
            int argumentIndex = arguments.IndexOf(Argument);
            int nextArgumentIndex = argumentIndex + 1;
            int tokensToPass = _tokens.Count - numberOfTokens;

            while (tokensToPass > 0 && nextArgumentIndex < arguments.Count)
            {
                CliArgument nextArgument = parent.Command.Arguments[nextArgumentIndex];
                ArgumentResult nextArgumentResult;

                if (SymbolResultTree.TryGetValue(nextArgument, out SymbolResult? symbolResult))
                {
                    nextArgumentResult = (ArgumentResult)symbolResult;
                }
                else
                {
                    // it might have not been parsed yet or due too few arguments, so we add it now
                    nextArgumentResult = new ArgumentResult(nextArgument, SymbolResultTree, Parent);
                    SymbolResultTree.Add(nextArgument, nextArgumentResult);
                }

                while (!nextArgumentResult.ArgumentLimitReached && tokensToPass > 0)
                {
                    CliToken toPass = _tokens[numberOfTokens];
                    _tokens.RemoveAt(numberOfTokens);
                    nextArgumentResult.AddToken(toPass);
                    --tokensToPass;
                }

                nextArgumentIndex++;
            }

            CommandResult rootCommand = parent;
            // When_tokens_are_passed_on_by_custom_parser_on_last_argument_then_they_become_unmatched_tokens
            while (tokensToPass > 0)
            {
                CliToken unmatched = _tokens[numberOfTokens];
                _tokens.RemoveAt(numberOfTokens);
                SymbolResultTree.AddUnmatchedToken(unmatched, parent, rootCommand);
                --tokensToPass;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(ArgumentResult)} {Argument.Name}: {string.Join(" ", Tokens.Select(t => $"<{t.Value}>"))}";

        /// <inheritdoc/>
        public override void AddError(string errorMessage)
        {
            SymbolResultTree.AddError(new ParseError(errorMessage, AppliesToPublicSymbolResult));
            _conversionResult = ArgumentConversionResult.Failure(this, errorMessage, ArgumentConversionResultType.Failed);
        }

        private ArgumentConversionResult ValidateAndConvert(bool useValidators)
        {
            if (!ArgumentArity.Validate(this, out ArgumentConversionResult? arityFailure))
            {
                return ReportErrorIfNeeded(arityFailure);
            }

            // There is nothing that stops user-defined Validator from calling ArgumentResult.GetValueOrDefault.
            // In such cases, we can't call the validators again, as it would create infinite recursion.
            // GetArgumentConversionResult => ValidateAndConvert => Validator
            //        => GetValueOrDefault => ValidateAndConvert (again)
            if (useValidators && Argument.HasValidators)
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

            return ReportErrorIfNeeded(
                ArgumentConversionResult.ArgumentConversionCannotParse(
                    this,
                    Argument.ValueType,
                    Tokens.Count > 0 
                        ? Tokens[0].Value
                        : ""));

            ArgumentConversionResult ReportErrorIfNeeded(ArgumentConversionResult result)
            {
                if (result.Result >= ArgumentConversionResultType.Failed)
                {
                    SymbolResultTree.AddError(new ParseError(result.ErrorMessage!, AppliesToPublicSymbolResult));
                }

                return result;
            }
        }

        /// <summary>
        /// Since Option.Argument is an internal implementation detail, this ArgumentResult applies to the OptionResult in public API if the parent is an OptionResult.
        /// </summary>
        private SymbolResult AppliesToPublicSymbolResult => 
            Parent is OptionResult optionResult ? optionResult : this;
    }
}
