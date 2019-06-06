// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolResult
    {
        private protected readonly List<Token> _tokens = new List<Token>();

        private ValidationMessages _validationMessages;

        private readonly Dictionary<IArgument, object> _defaultArgumentValues = new Dictionary<IArgument, object>();

        private protected SymbolResult(
            ISymbol symbol, 
            Token token, 
            SymbolResult parent = null)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token ?? throw new ArgumentNullException(nameof(token));

            Parent = parent;
        }

        [Obsolete("Use the ArgumentResults property instead")]
        internal ArgumentConversionResult ArgumentConversionResult
        {
            get
            {
                var argument =  Symbol switch {
                    IOption o => o.Argument,
                    ICommand c => c.Argument,
                    IArgument a => a,
                    _ => null
                };


                return ArgumentConversionResults.SingleOrDefault() ??
                       ArgumentConversionResult.None(argument);
            }
        }

        internal ArgumentConversionResultSet ArgumentConversionResults
        {
            get
            {
                var results = Children
                              .OfType<ArgumentResult>()
                              .Select(r => Parse(r, r.Argument));

                var resultSet = new ArgumentConversionResultSet();

                foreach (var result in results)
                {
                    resultSet.Add(result);
                }

                return resultSet;

            }
        }

        [Obsolete("Use the Tokens property instead. The Arguments property will be removed in a later version.")]
        public IReadOnlyCollection<string> Arguments => 
            Tokens.Select(t => t.Value).ToArray();

        public string ErrorMessage { get; set; }

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public string Name => Symbol.Name;

        public SymbolResult Parent { get; }

        public CommandResult ParentCommandResult => Parent as CommandResult;

        public ISymbol Symbol { get; }

        public Token Token { get; }

        public IReadOnlyList<Token> Tokens => _tokens;

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity <= 0;

        private protected virtual int RemainingArgumentCapacity =>
            MaximumArgumentCapacity() - Tokens.Count;

        internal int MaximumArgumentCapacity() =>
            Symbol.Arguments()
                  .Sum(a => a.Arity.MaximumNumberOfValues);

        protected internal ValidationMessages ValidationMessages
        {
            get
            {
                if (_validationMessages == null)
                {
                    if (Parent == null)
                    {
                        _validationMessages = ValidationMessages.Instance;
                    }
                    else
                    {
                        _validationMessages = Parent.ValidationMessages;
                    }
                }

                return _validationMessages;
            }
            set => _validationMessages = value;
        }

        internal void AddToken(Token token) => _tokens.Add(token);

        internal object GetDefaultValueFor(IArgument argument)
        {
            return _defaultArgumentValues.GetOrAdd(
                argument,
                a => a.GetDefaultValue());
        }

        internal bool UseDefaultValueFor(IArgument argument)
        {
            if (this is OptionResult optionResult &&
                optionResult.IsImplicit)
            {
                return true;
            }

            if (this is CommandResult &&
                Children.ResultFor(argument)?.Token is ImplicitToken)
            {
                return true;
            }

            return _defaultArgumentValues.ContainsKey(argument);
        }

        public override string ToString() => $"{GetType().Name}: {Token}";

        internal static ArgumentConversionResult Parse(
            ArgumentResult argumentResult,
            IArgument argument) =>
            Parse(argumentResult.Parent, argument);

        internal static ArgumentConversionResult Parse(
            SymbolResult symbolResult,
            IArgument argument)
        {
            if (ShouldCheckArity() &&
                     ArgumentArity.Validate(symbolResult,
                                            argument,
                                            argument.Arity.MinimumNumberOfValues,
                                            argument.Arity.MaximumNumberOfValues) is FailedArgumentConversionResult failedResult)
            {
                return failedResult;
            }

            if (symbolResult.UseDefaultValueFor(argument))
            {
                var defaultValueFor = symbolResult.GetDefaultValueFor(argument);

                return ArgumentConversionResult.Success(argument, defaultValueFor);
            }

            if (argument is Argument a &&
                a.ConvertArguments != null)
            {
                var argumentResult = symbolResult.Children.ResultFor(argument);

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
                    return ArgumentConversionResult.Failure(argument, symbolResult.ErrorMessage ?? $"Invalid: {symbolResult.Token} {string.Join(" ", symbolResult.Arguments)}");
                }
            }

            switch (argument.Arity.MaximumNumberOfValues)
            {
                case 0:
                    return ArgumentConversionResult.Success(argument, null);

                case 1:
                    return ArgumentConversionResult.Success(argument, symbolResult.Arguments.SingleOrDefault());

                default:
                    return ArgumentConversionResult.Success(argument, symbolResult.Arguments);
            }

            bool ShouldCheckArity()
            {
                return !(symbolResult is OptionResult optionResult &&
                       optionResult.IsImplicit);
            }
        }

        internal ParseError UnrecognizedArgumentError(Argument argument)
        {
            if (argument.AllowedValues?.Count > 0 &&
                Tokens.Count > 0)
            {
                foreach (var token in Tokens)
                {
                    if (!argument.AllowedValues.Contains(token.Value))
                    {
                        return new ParseError(
                            ValidationMessages
                                .UnrecognizedArgument(token.Value, argument.AllowedValues),
                            this);
                    }
                }
            }

            return null;
        }

        internal ParseError CustomError(Argument argument)
        {
            foreach (var symbolValidator in argument.SymbolValidators)
            {
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this);
                }
            }

            return null;
        }
    }
}
